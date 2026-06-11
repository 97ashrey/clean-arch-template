namespace Company.Service.RestApi.Api.Accounts.V1.Contracts;

public record class AccountOrder
{
    public required Guid Id { get; init; }

    public required Guid TenantId { get; init; }

    public required Guid? AccountId { get; init; }

    public required AccountDetails AccountDetails { get; init; }

    public required ContactInformation ContactInformation { get; init; }

    public required AccountOrderStatus Status { get; init; }

    public required DateTime CreatedDate { get; init; }
}

public record AccountDetails
{
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required AccountTier Tier { get; init; }
    public required Guid InvoiceAddressId { get; init; }
}

public record ContactInformation
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required string PhoneNumber { get; init; }
}

public enum AccountOrderStatus
{
    Pending,
    Processing,
    Completed,
}

internal static class AccountOrdersV1Mapping
{
    extension(Domain.Entities.AccountOrder accountOrder)
    {
        public AccountOrder ToV1() =>
            new()
            {
                Id = accountOrder.Id,
                TenantId = accountOrder.TenantId,
                AccountId = accountOrder.AccountId,
                AccountDetails = new()
                {
                    Name = accountOrder.AccountDetails.Name,
                    Email = accountOrder.AccountDetails.Email,
                    Tier = (AccountTier)accountOrder.AccountDetails.Tier,
                    InvoiceAddressId = accountOrder.AccountDetails.InvoiceAddressId
                },
                ContactInformation = new()
                {
                    FirstName = accountOrder.ContactInformation.FirstName,
                    LastName = accountOrder.ContactInformation.LastName,
                    Email = accountOrder.ContactInformation.Email,
                    PhoneNumber = accountOrder.ContactInformation.PhoneNumber
                },
                Status = (AccountOrderStatus)accountOrder.Status,
                CreatedDate = accountOrder.CreatedDate
            };
    }
}
