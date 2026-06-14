namespace Company.Service.RestApi.Api.Accounts.V1.Contracts;

public record class Account
{
    public required Guid Id { get; init; }

    public required Guid TenantId { get; init; }

    public required string Name { get; init; }

    public required string Email { get; init; }

    public required AccountTier Tier { get; init; }

    public required AccountStatus Status { get; init; }

    public required DateTime? SuspendedDate { get; init; }

    public required Guid InvoiceAddressId { get; init; }
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

internal static class AccountsV1Mapping
{
    extension(Domain.Entities.Account account)
    {
        public Account ToV1() =>
            new()
            {
                Id = account.Id,
                TenantId = account.TenantId,
                Name = account.Name,
                Email = account.Email,
                Tier = (AccountTier)account.Tier,
                Status = (AccountStatus)account.Status,
                SuspendedDate = account.SuspendedDate,
                InvoiceAddressId = account.InvoiceAddressId
            };
    }
}