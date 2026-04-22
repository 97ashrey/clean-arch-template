using Company.Service.Application.Common.Interfaces.Persistence;
using Company.Service.Application.Common.Requests;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;
using FluentValidation;
using MassTransit;

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
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateAccountOrderCommandHandler(IApplicationDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
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
                .Bind(ci =>
                    AccountOrder.CreateNew(
                        tenantId: request.TenantId,
                        accountDetails: ad,
                        contactInformation: ci,
                        createdDate: DateTime.UtcNow
                    )
                )
            )
            .MapError<ApplicationError>(error =>
                new ValidationError()
                {
                    Message = "Validation failed!.",
                    Failures = error.Failures.Select(f => new ValidationFailure(f.PropertyName, f.Errors)).ToArray()
                }
            )
            .TapAsync(async accountOrder =>
            {
                _dbContext.AccountOrders.Add(accountOrder);

                await _publishEndpoint.Publish(CreateAccountOrderCreatedEvent(accountOrder));

                await _dbContext.SaveChangesAsync(cancellationToken);
            });
    }

    private static IntegrationEvents.V1.Accounts.AccountOrderCreatedEvent CreateAccountOrderCreatedEvent(AccountOrder accountOrder)
        => new(
            AccountOrderId: accountOrder.Id,
            TenantId: accountOrder.TenantId,
            AccountDetails: new(
                Name: accountOrder.AccountDetails.Name,
                Email: accountOrder.AccountDetails.Email,
                Tier: (IntegrationEvents.V1.Shared.AccountTier)accountOrder.AccountDetails.Tier
            ),
            ContactInformation: new(
                accountOrder.ContactInformation.FirstName,
                accountOrder.ContactInformation.LastName,
                accountOrder.ContactInformation.Email,
                accountOrder.ContactInformation.PhoneNumber
            ),
            CreatedDate: accountOrder.CreatedDate
        );

}