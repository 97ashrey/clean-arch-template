using Company.Service.Application.IntegrationEvents.V1.Shared;

namespace Company.Service.Application.IntegrationEvents.V1.Subscriptions;

public record SubscriptionCreatedEvent(
    Guid SubscriptionId,
    Guid AccountId,
    string Name,
    string FriendlyName,
    Price PurchasePrice,
    BillCycle BillCycle,
    DateTime StartDate,
    DateTime EndDate,
    Guid ProductId,
    DateTime CreatedDate
);