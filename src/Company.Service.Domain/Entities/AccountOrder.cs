using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Common.Types.Errors;
using Company.Service.Domain.Common.Types.Utils;

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

    public DateTime? CompletedDate { get; private set; }

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

    public static Result<AccountOrder, ValidationError> CreateNew(Guid tenantId, string accountName, AccountTier tier, ContactInformation contactInformation, DateTime createdDate, Guid invoiceAddressId)
    {
        return Validate.ExecuteRules(
            Validate.NotEmpty(accountName, nameof(accountName))
        ).MapToValueResult(new AccountOrder
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                AccountName = accountName,
                Tier = tier,
                ContactInformation = contactInformation,
                Status = AccountOrderStatus.Pending,
                CreatedDate = createdDate,
                InvoiceAddressId = invoiceAddressId
            }
        );
    }

    public Result<InvalidOperationError> StartProcessing()
    {
        if (Status != AccountOrderStatus.Pending)
        {
            return new InvalidOperationError($"Can't start processing order. It is not in {AccountOrderStatus.Pending} state!");
        }

        Status = AccountOrderStatus.Processing;

        return new();
    }

    public Result<InvalidOperationError> Complete(Account account, DateTime completedDate)
    {
        if (Status != AccountOrderStatus.Processing)
        {
            return new InvalidOperationError($"Can't complete order. It is not in {AccountOrderStatus.Processing} state!.");
        }

        if (completedDate < CreatedDate)
        {
            return new InvalidOperationError($"Can't complete order. Completed Date {completedDate} is before Created Date {CreatedDate}.");
        }

        Status = AccountOrderStatus.Completed;
        CompletedDate = completedDate;
        AccountId = account.Id;

        return new();
    }
}

public enum AccountOrderStatus
{
    Pending,
    Processing,
    Completed,
}