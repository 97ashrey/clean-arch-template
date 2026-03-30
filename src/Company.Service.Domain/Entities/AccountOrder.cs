using Company.Service.Domain.Common;

namespace Company.Service.Domain.Entities;

public class AccountOrder
{
    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public string AccountName { get; private set; } = string.Empty;

    public AccountTier Tier { get; private set; }

    public ContactInformation ContactInformation { get; private set; } = null!;

    public AccountOrderStatus Status { get; private set; }

    public DateTime CreatedDate { get; private set; }

    public Guid? AccountId { get; private set; }

    public Guid InvoiceAddressId { get; private set; }

    internal AccountOrder(
        Guid id,
        Guid tenantId,
        string accountName,
        AccountTier tier,
        ContactInformation contactInformation,
        AccountOrderStatus status,
        DateTime createdDate,
        Guid? accountId,
        Guid invoiceAddressId)
    {
        Id = id;
        TenantId = tenantId;
        AccountName = accountName;
        Tier = tier;
        ContactInformation = contactInformation;
        Status = status;
        CreatedDate = createdDate;
        AccountId = accountId;
        InvoiceAddressId = invoiceAddressId;
    }

    private AccountOrder(){}

    public void StartProcessing()
    {
        if (Status != AccountOrderStatus.Pending)
        {
            throw new InvalidOperationException($"Can't start processing order. It is not in {AccountOrderStatus.Pending} state!");
        }

        Status = AccountOrderStatus.Processing;
    }

    public void Complete(Guid accountId)
    {
        if (Status != AccountOrderStatus.Processing)
        {
            throw new InvalidOperationException($"Can't complete order. It is not in {AccountOrderStatus.Processing} state!.");
        }

        Status = AccountOrderStatus.Completed;
        AccountId = accountId;
    }
}

public enum AccountOrderStatus
{
    Pending,
    Processing,
    Completed,
}

public record ContactInformation(string FirstName, string LastName, string Email, string PhoneNumber);

public static class AccountOrderConstruction
{
    extension(AccountOrder)
    {
        public static AccountOrder CreateNew(Guid tenantId, string accountName, AccountTier tier, ContactInformation contactInformation, DateTime createdDate, Guid invoiceAddressId)
        {
            Guard.AgainstNullOrEmpty(accountName, nameof(accountName));
            Guard.AgainstNullOrEmpty(contactInformation.FirstName, nameof(contactInformation.FirstName));
            Guard.AgainstNullOrEmpty(contactInformation.LastName, nameof(contactInformation.LastName));
            Guard.AgainstNullOrEmpty(contactInformation.Email, nameof(contactInformation.Email));
            Guard.AgainstNullOrEmpty(contactInformation.PhoneNumber, nameof(contactInformation.PhoneNumber));

            return new AccountOrder(id: Guid.NewGuid(), tenantId, accountName, tier, contactInformation, AccountOrderStatus.Pending, createdDate, accountId: null, invoiceAddressId);
        }
    }
}