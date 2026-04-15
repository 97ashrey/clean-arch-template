using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Common.Types.Errors;
using Company.Service.Domain.Common.Types.Utils;

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

    private Subscription() { }

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

    public static Result<Subscription, ValidationError> CreateNew(
            Guid accountId,
            string name,
            string friendlyName,
            Price purchasePrice,
            BillCycle billCycle,
            DateTime startDate,
            DateTime endDate,
            Guid productId)
    {
        return Validate.ExecuteRules(
            Validate.NotEmpty(accountId, nameof(accountId)),
            Validate.NotEmpty(name, nameof(name)),
            Validate.NotEmpty(friendlyName, nameof(friendlyName)),
            Validate.NotZero(purchasePrice.Value, $"{nameof(purchasePrice)}.{nameof(purchasePrice.Value)}"),
            Validate.NotEmpty(purchasePrice.Currency, $"{nameof(purchasePrice)}.{nameof(purchasePrice.Currency)}"),
            Validate.Must(() =>
            {
                if (startDate > endDate)
                {
                    return new SingleFailure($"{nameof(startDate)}-{nameof(endDate)}", $"{nameof(startDate)} can't be greater than {nameof(endDate)}!");
                }

                return null;
            })
        )
        .MapToValueResult(new Subscription(
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
            )
        );
    }

    public Result<ValidationError> Update(string friendlyName)
    {
        return Validate.ExecuteRules(
            Validate.NotEmpty(friendlyName, nameof(friendlyName))
        ).Bind(() =>
        {
            FriendlyName = friendlyName;
            return new();
        });
    }

    public Result<InvalidOperationError> Suspend(DateTime suspendedDate)
    {
        if (Status != SubscriptionStatus.Active)
        {
            return new InvalidOperationError($"Can't suspend the subscription it is not in {SubscriptionStatus.Active} state!");
        }

        Status = SubscriptionStatus.Suspended;
        SuspendedDate = suspendedDate;

        return new();
    }

    public Result<InvalidOperationError> ReActivate()
    {
        if (Status != SubscriptionStatus.Suspended)
        {
            return new InvalidOperationError($"Can't re-activate the subscription it is not in {SubscriptionStatus.Suspended} state!");
        }

        Status = SubscriptionStatus.Active;
        SuspendedDate = null;
        return new();
    }

    public Result<InvalidOperationError> Cancel()
    {
        if (Status != SubscriptionStatus.Suspended)
        {
            return new InvalidOperationError($"Can't cancel the subscription it is not in {SubscriptionStatus.Suspended} state!");
        }

        Status = SubscriptionStatus.Canceled;
        return new();
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