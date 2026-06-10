using Company.Service.Application.Common.Interfaces.Persistence;
using Company.Service.Application.Common.Requests;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;
using FluentValidation;

namespace Company.Service.Application.InvoiceAddresses.Commands;

public record CreateInvoiceAddressCommand : ApplicationRequest<InvoiceAddress>
{
    public required Guid TenantId { get; init; }
    public required string Name { get; init; }
    public required AddressCommand Address { get; init; }

    public record AddressCommand
    {
        public required string Street { get; init; }
        public required string City { get; init; }
        public required string ZipCode { get; init; }
        public required string Country { get; init; }
        public required string Number { get; init; }
    }
}

internal class CreateInvoiceAddressCommandValidator : AbstractValidator<CreateInvoiceAddressCommand>
{
    public CreateInvoiceAddressCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Address.Street).NotEmpty();
        RuleFor(x => x.Address.City).NotEmpty();
        RuleFor(x => x.Address.ZipCode).NotEmpty();
        RuleFor(x => x.Address.Country).NotEmpty();
        RuleFor(x => x.Address.Number).NotEmpty();
    }
}

internal class CreateInvoiceAddressCommandHandler : IApplicationRequestHandler<CreateInvoiceAddressCommand, InvoiceAddress>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateInvoiceAddressCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<ValueResult<InvoiceAddress, ApplicationError>> Handle(CreateInvoiceAddressCommand request, CancellationToken cancellationToken)
    {
        return await Address
            .CreateNew(
                request.Address.Country,
                request.Address.City,
                request.Address.ZipCode,
                request.Address.Street,
                request.Address.Number)
            .Bind(address =>
            {
                return InvoiceAddress.CreateNew(
                    tenantId: request.TenantId,
                    name: request.Name,
                    address: address);
            })
            .MatchAsync<ValueResult<InvoiceAddress, ApplicationError>>(
                async invoiceAddress =>
                {
                    _dbContext.InvoiceAdresses.Add(invoiceAddress);

                    await _dbContext.SaveChangesAsync(cancellationToken);

                    return invoiceAddress;
                },
                async failure => new ValidationError()
                {
                    Message = "Validation failed!",
                    Failures = failure.Failures.Select(f => new ValidationFailure(f.PropertyName, f.Errors)).ToArray()
                }
            );
    }
}