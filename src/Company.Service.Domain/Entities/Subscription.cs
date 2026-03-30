using Company.Service.Domain.Common;

namespace Company.Service.Domain.Entities;

public class Subscription
{
    public Guid Id { get; private set; }

    public Guid AccountId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string FriendlyName { get; private set; } = string.Empty;

    public Price PurchasePrice { get; private set; } = null!;

    public BillCycle BillCycle { get; private set; }

    public DateTime StartDate { get; private set; }

    public DateTime EndDate { get; private set; }

    public SubscriptionStatus Status { get; private set; }

    public DateTime? SuspendedDate { get; private set; }

    public Guid ProductId { get; private set; }

    private Subscription() {}

    internal Subscription(
        Guid id,
        Guid accountId,
        string name,
        string friendlyName,
        Price purchasePrice,
        BillCycle billCycle,
        DateTime startDate,
        DateTime endDate,
        SubscriptionStatus status,
        DateTime? suspendedDate,
        Guid productId)
    {
        Id = id;
        AccountId = accountId;
        Name = name;
        FriendlyName = friendlyName;
        PurchasePrice = purchasePrice;
        BillCycle = billCycle;
        StartDate = startDate;
        EndDate = endDate;
        Status = status;
        SuspendedDate = suspendedDate;
        ProductId = productId;
    }

    public void Update(string friendlyName)
    {
        Guard.AgainstNullOrEmpty(friendlyName, nameof(friendlyName));

        FriendlyName = friendlyName;
    }

    public void Suspend(DateTime suspendedDate)
    {
        if (Status != SubscriptionStatus.Active)
        {
            throw new InvalidOperationException($"Can't suspend the subscription it is not in {SubscriptionStatus.Active} state!");
        }

        Status = SubscriptionStatus.Suspended;
        SuspendedDate = suspendedDate;
    }

    public void ReActivate()
    {
        if (Status != SubscriptionStatus.Suspended)
        {
            throw new InvalidOperationException($"Can't re-activate the subscription it is not in {SubscriptionStatus.Suspended} state!");
        }

        Status = SubscriptionStatus.Active;
        SuspendedDate = null;
    }

    public void Cancel()
    {
        if (Status != SubscriptionStatus.Suspended)
        {
            throw new InvalidOperationException($"Can't cancel the subscription it is not in {SubscriptionStatus.Suspended} state!");
        }

        Status = SubscriptionStatus.Canceled;
    }
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

public record Price(decimal Value, string Currency);

public static class SubscriptionConstruction
{
    extension(Subscription)
    {
        public static Subscription CreateNew(
            Guid accountId,
            string name,
            string friendlyName,
            Price purchasePrice,
            BillCycle billCycle,
            DateTime startDate,
            DateTime endDate,
            Guid productId)
        {
            Guard.AgainstNullOrEmpty(name, nameof(name));
            Guard.AgainstNullOrEmpty(friendlyName, nameof(friendlyName));

            Guard.AgainstZero(purchasePrice.Value, $"{nameof(purchasePrice)}.{nameof(purchasePrice.Value)}");
            Guard.AgainstNullOrEmpty(purchasePrice.Currency, $"{nameof(purchasePrice)}.{nameof(purchasePrice.Currency)}");

            if (startDate > endDate)
            {
                throw new ArgumentException($"{nameof(startDate)} can't be greater than {nameof(endDate)}!");
            }

            return new Subscription(
                id: Guid.NewGuid(),
                accountId,
                name,
                friendlyName,
                purchasePrice,
                billCycle,
                startDate,
                endDate,
                SubscriptionStatus.Active,
                suspendedDate: null,
                productId
            );
        }
    }
}