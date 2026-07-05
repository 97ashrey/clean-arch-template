//__EXAMPLE_START__
using Company.Service.Application.Common.Interfaces.Persistence;
using Company.Service.Application.Common.Requests;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Application.Common.Utils;
using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;
using FluentValidation;
using MassTransit;

namespace Company.Service.Application.Features.InvoiceAddresses.Commands;

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
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly TimeProvider _timeProvider;

    public CreateInvoiceAddressCommandHandler(IApplicationDbContext dbContext, IPublishEndpoint publishEndpoint, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _timeProvider = timeProvider;
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
            .MapError<ApplicationError>(error => error.ToAppValidationError())
            .TapAsync(async invoiceAddress =>
            {
                _dbContext.InvoiceAdresses.Add(invoiceAddress);

                await _publishEndpoint.Publish(CreateInvoiceAddressCreatedEvent(invoiceAddress));

                await _dbContext.SaveChangesAsync(cancellationToken);
            });
    }

    private IntegrationEvents.V1.InvoiceAddresses.InvoiceAddressCreatedEvent CreateInvoiceAddressCreatedEvent(InvoiceAddress invoiceAddress)
    {
        return new(
            InvoiceAddressId: invoiceAddress.Id,
            TenantId: invoiceAddress.TenantId,
            Name: invoiceAddress.Name,
            Address: new IntegrationEvents.V1.Shared.Address(
                invoiceAddress.Address.Country,
                invoiceAddress.Address.City,
                invoiceAddress.Address.ZipCode,
                invoiceAddress.Address.Street,
                invoiceAddress.Address.Number
            ),
            CreatedDate: _timeProvider.GetUtcNowDateTime()
        );
    }
}
//__EXAMPLE_END__