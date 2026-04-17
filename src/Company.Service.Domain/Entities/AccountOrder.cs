using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Common.Types.Errors;
using Company.Service.Domain.Common.Types.Utils;
using Company.Service.Domain.ValueObjects;

namespace Company.Service.Domain.Entities;

public class AccountOrder
{
    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public AccountDetails AccountDetails { get; private set; } = null!;

    public Guid? AccountId { get; private set; }

    public ContactInformation ContactInformation { get; private set; } = null!;

    public AccountOrderStatus Status { get; private set; }

    public DateTime CreatedDate { get; private set; }

    public DateTime? CompletedDate { get; private set; }

    internal AccountOrder(
        Guid id,
        Guid tenantId,
        AccountDetails accountDetails,
        Guid? accountId,
        ContactInformation contactInformation,
        AccountOrderStatus status,
        DateTime createdDate,
        DateTime? completedDate
        )
    {
        Id = id;
        TenantId = tenantId;
        AccountDetails = accountDetails;
        AccountId = accountId;
        ContactInformation = contactInformation;
        Status = status;
        CreatedDate = createdDate;
        CompletedDate = completedDate;
    }

    private AccountOrder() { }

    public static Result<AccountOrder, ValidationError> CreateNew(Guid tenantId, AccountDetails accountDetails, ContactInformation contactInformation, DateTime createdDate)
    {
        return Validate.ExecuteRules(
            Validate.NotEmpty(tenantId, nameof(tenantId))
        ).MapToValueResult(new AccountOrder
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AccountDetails = accountDetails,
            ContactInformation = contactInformation,
            Status = AccountOrderStatus.Pending,
            CreatedDate = createdDate,
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