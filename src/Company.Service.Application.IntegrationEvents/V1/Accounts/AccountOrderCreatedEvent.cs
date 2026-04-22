using Company.Service.Application.IntegrationEvents.V1.Shared;

namespace Company.Service.Application.IntegrationEvents.V1.Accounts;

public record AccountOrderCreatedEvent(
    Guid AccountOrderId,
    Guid TenantId,
    AccountDetails AccountDetails,
    ContactInformation ContactInformation,
    DateTime CreatedDate
);