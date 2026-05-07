using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Common.Types.Errors;
using Company.Service.Domain.Common.Utils;

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

    private Account() { }

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

    public static ValueResult<Account, ValidationError> CreateNew(Guid tenantId, string name, string email, AccountTier tier, Guid invoiceAddressId)
    {
        return Validate.ExecuteRules(
            Validate.NotEmpty(tenantId, nameof(tenantId)),
            Validate.NotEmpty(name, nameof(name)),
            Validate.NotEmpty(email, nameof(email)),
            Validate.NotEmpty(invoiceAddressId, nameof(invoiceAddressId))
        ).MapToValueResult(new Account()
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            Email = email,
            Tier = tier,
            Status = AccountStatus.Active,
            SuspendedDate = null,
            InvoiceAddressId = invoiceAddressId
        }
        );
    }

    public Result<ValidationError> ChangeName(string name)
    {
        return Validate.ExecuteRules(
            Validate.NotEmpty(name, nameof(name))
        ).Bind(() =>
        {
            Name = name;
            return new();
        });
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