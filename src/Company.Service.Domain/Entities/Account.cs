using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Common.Types.Errors;

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

    public Result<ValidationError> ChangeName(string name)
    {
        var error = Validate.ExecuteRules(
            Validate.NotEmpty(name, nameof(name))
        );

        if (error is not null)
        {
            return error;
        }

        Name = name;

        return new();
    }

    public Result<ValidationError> ChangeEmail(string email)
    {
        var error = Validate.ExecuteRules(
            Validate.NotEmpty(email, nameof(email))
        );

        if (error is not null)
        {
            return error;
        }

        Email = email;

        return new();
    }

    public Result<InvalidOperationError> Suspend(DateTime suspendedDate)
    {
        if (Status != AccountStatus.Active)
        {
            return new InvalidOperationError($"Can't suspend the account it is not in {AccountStatus.Active} state!");
        }

        Status = AccountStatus.Suspended;
        SuspendedDate = suspendedDate;

        return new();
    }

    public Result<InvalidOperationError> ReActivate()
    {
        if (Status != AccountStatus.Suspended)
        {
            return new InvalidOperationError($"Can't re-activate the account it is not in {AccountStatus.Suspended} state!");
        }

        Status = AccountStatus.Active;
        SuspendedDate = null;

        return new();
    }

    public Result<InvalidOperationError> Remove()
    {
        if (Status != AccountStatus.Suspended)
        {
            return new InvalidOperationError($"Can't remove the account it is not in {AccountStatus.Suspended} state!");
        }

        Status = AccountStatus.Removed;

        return new();
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
        public static Result<Account, ValidationError> CreateNew(Guid tenantId, string name, string email, AccountTier tier, Guid invoiceAddressId)
        {
            var error = Validate.ExecuteRules(
                Validate.NotEmpty(tenantId, nameof(tenantId)),
                Validate.NotEmpty(name, nameof(name)),
                Validate.NotEmpty(email, nameof(email)),
                Validate.NotEmpty(invoiceAddressId, nameof(invoiceAddressId))
            );

            if (error is not null)
            {
                return error;
            }

            return new Account(id: Guid.NewGuid(), tenantId, name, email, tier, AccountStatus.Active, suspendedDate: null, invoiceAddressId);
        }
    }
}
