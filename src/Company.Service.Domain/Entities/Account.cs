using Company.Service.Domain.Common;

namespace Company.Service.Domain.Entities;

public class Account
{
    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public AccountTier Tier { get; private set; }

    public AccountStatus Status { get; private set; }

    public DateTime? SuspendedDate { get; private set; }

    public Guid InvoiceAddressId { get; private set; }

    internal Account(
        Guid id,
        Guid tenantId,
        string name,
        string email,
        AccountTier tier,
        AccountStatus status,
        DateTime? suspendedDate,
        Guid invoiceAddressId)
    {
        Id = id;
        TenantId = tenantId;
        Name = name;
        Email = email;
        Tier = tier;
        Status = status;
        SuspendedDate = suspendedDate;
        InvoiceAddressId = invoiceAddressId;
    }

    public void Update(string name)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));

        Name = name;
    }

    public void Suspend(DateTime suspendedDate)
    {
        if (Status != AccountStatus.Active)
        {
            throw new InvalidOperationException($"Can't suspend the account it is not in {AccountStatus.Active} state!");
        }

        Status = AccountStatus.Suspended;
        SuspendedDate = suspendedDate;
    }

    public void ReActivate()
    {
        if (Status != AccountStatus.Suspended)
        {
            throw new InvalidOperationException($"Can't re-activate the account it is not in {AccountStatus.Suspended} state!");
        }

        Status = AccountStatus.Active;
        SuspendedDate = null;
    }

    public void Remove()
    {
        if (Status != AccountStatus.Suspended)
        {
            throw new InvalidOperationException($"Can't remove the account it is not in {AccountStatus.Suspended} state!");
        }

        Status = AccountStatus.Removed;
    }
}

public enum AccountStatus
{
    Active,
    Suspended,
    Removed
}

public enum AccountTier
{
    Individual,
    Business,
    Enterprise
}

public static class AccountConstruction
{
    extension(Account)
    {
        public static Account CreateNew(Guid tenantId, string name, string email, AccountTier tier, Guid invoiceAddressId)
        {
            Guard.AgainstNullOrEmpty(name, nameof(name));
            Guard.AgainstNullOrEmpty(email, nameof(email));

            return new Account(id: Guid.NewGuid(), tenantId, name, email, tier, AccountStatus.Active, suspendedDate: null, invoiceAddressId);
        }
    }
}
