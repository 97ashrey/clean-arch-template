using Company.Service.Application.Common.Interfaces.Persistence;
using Company.Service.Application.Common.Requests;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;
using FluentValidation;

namespace Company.Service.Application.Accounts.Commands;

public record CreateAccountOrderCommand : ApplicationRequest<AccountOrder>
{
    public required Guid TenantId { get; init; }
    public required AccountDetailsCommand AccountDetails { get; init; }
    public required ContactInformationCommand ContactInformation { get; init; }

    public record ContactInformationCommand
    {
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public required string Email { get; init; }
        public required string PhoneNumber { get; init; }
    }

    public record AccountDetailsCommand
    {
        public required string Name { get; init; }
        public required string Email { get; init; }
        public required AccountTier Tier { get; init; }
        public required Guid InvoiceAddressId { get; init; }
    }
}

internal class CreateAccountOrderCommandValidator : AbstractValidator<CreateAccountOrderCommand>
{
    public CreateAccountOrderCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();

        RuleFor(x => x.AccountDetails.Name).NotEmpty();
        RuleFor(x => x.AccountDetails.Email).EmailAddress();
        RuleFor(x => x.AccountDetails.Tier).IsInEnum();
        RuleFor(x => x.AccountDetails.InvoiceAddressId).NotEmpty();

        RuleFor(x => x.ContactInformation.FirstName).NotEmpty();
        RuleFor(x => x.ContactInformation.LastName).NotEmpty();
        RuleFor(x => x.ContactInformation.Email).EmailAddress();
        RuleFor(x => x.ContactInformation.PhoneNumber).NotEmpty();
    }
}

internal class CreateAccountOrderCommandHandler : IApplicationRequestHandler<CreateAccountOrderCommand, AccountOrder>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateAccountOrderCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<Result<AccountOrder, ApplicationError>> Handle(CreateAccountOrderCommand request, CancellationToken cancellationToken)
    {
        return await
            AccountDetails.CreateNew(
                request.AccountDetails.Name,
                request.AccountDetails.Email,
                request.AccountDetails.Tier,
                request.AccountDetails.InvoiceAddressId
            )
            .Bind(ad =>
                ContactInformation.CreateNew(
                    request.ContactInformation.FirstName,
                    request.ContactInformation.LastName,
                    request.ContactInformation.Email,
                    request.ContactInformation.PhoneNumber
                )
                .Map(ci => (ad, ci))
            )
            .Bind(adAndCi =>
                AccountOrder.CreateNew(
                    tenantId: request.TenantId,
                    accountDetails: adAndCi.ad,
                    contactInformation: adAndCi.ci,
                    createdDate: DateTime.UtcNow
                )
            )
            .MatchAsync<Result<AccountOrder, ApplicationError>>(
                async accountOrder =>
                {
                    _dbContext.AccountOrders.Add(accountOrder);

                    // publish integration event

                    await _dbContext.SaveChangesAsync(cancellationToken);

                    return accountOrder;
                },
                async failure => new ValidationError()
                {
                    Message = "Validation failed!.",
                    Failures = failure.Failures.Select(f => new ValidationFailure(f.PropertyName, f.Errors)).ToArray()
                }
            );
    }

}