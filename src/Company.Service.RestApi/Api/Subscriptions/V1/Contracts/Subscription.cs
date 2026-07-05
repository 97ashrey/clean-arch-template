namespace Company.Service.RestApi.Api.Subscriptions.V1.Contracts;

public record class Subscription
{
    public required Guid Id { get; init; }
    public required Guid AccountId { get; init; }
    public required string Name { get; init; }
    public required string FriendlyName { get; init; }
    public required Price PurchasePrice { get; init; }
    public required BillCycle BillCycle { get; init; }
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }
    public required SubscriptionStatus Status { get; init; }
    public required DateTime? SuspendedDate { get; init; }
    public required Guid ProductId { get; init; }
}

public enum SubscriptionStatus
{
    Active,
    Suspended,
    Canceled
}

public enum BillCycle
{
    Monthly,
    Yearly
}

internal static class SubscriptionsV1Mapping
{
    extension(Domain.Entities.Subscription subscription)
    {
        public Subscription ToV1() =>
            new()
            {
                Id = subscription.Id,
                AccountId = subscription.AccountId,
                Name = subscription.Name,
                FriendlyName = subscription.FriendlyName,
                PurchasePrice = new Price { Value = subscription.PurchasePrice.Value, Currency = subscription.PurchasePrice.Currency },
                BillCycle = (BillCycle)subscription.BillCycle,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                Status = (SubscriptionStatus)subscription.Status,
                SuspendedDate = subscription.SuspendedDate,
                ProductId = subscription.ProductId
            };
    }
}