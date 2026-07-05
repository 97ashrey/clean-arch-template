//__EXAMPLE_START__
using Company.Service.Application.IntegrationEvents.V1.Shared;

namespace Company.Service.Application.IntegrationEvents.V1.InvoiceAddresses;

public record InvoiceAddressCreatedEvent(
    Guid InvoiceAddressId,
    Guid TenantId,
    string Name,
    Address Address,
    DateTime CreatedDate
);
//__EXAMPLE_END__