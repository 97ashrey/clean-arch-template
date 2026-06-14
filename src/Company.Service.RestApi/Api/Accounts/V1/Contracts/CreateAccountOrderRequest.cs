using Company.Service.Application.Accounts.Commands;

namespace Company.Service.RestApi.Api.Accounts.V1.Contracts;

public record class CreateAccountOrderRequest
{
    public required Guid TenantId { get; init; }

    public required AccountDetailsRequest AccountDetails { get; init; }

    public required ContactInformationRequest ContactInformation { get; init; }

    public record AccountDetailsRequest
    {
        public required string Name { get; init; }
        public required string Email { get; init; }
        public required AccountTier Tier { get; init; }
        public required Guid InvoiceAddressId { get; init; }
    }

    public record ContactInformationRequest
    {
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public required string Email { get; init; }
        public required string PhoneNumber { get; init; }
    }
}

internal static class CreateAccountOrderRequestToCommandMapping
{
    extension(CreateAccountOrderRequest request)
    {
        public CreateAccountOrderCommand ToCommand() =>
            new()
            {
                TenantId = request.TenantId,
                AccountDetails = new()
                {
                    Name = request.AccountDetails.Name,
                    Email = request.AccountDetails.Email,
                    Tier = (Domain.Entities.AccountTier)request.AccountDetails.Tier,
                    InvoiceAddressId = request.AccountDetails.InvoiceAddressId
                },
                ContactInformation = new()
                {
                    FirstName = request.ContactInformation.FirstName,
                    LastName = request.ContactInformation.LastName,
                    Email = request.ContactInformation.Email,
                    PhoneNumber = request.ContactInformation.PhoneNumber
                }
            };
    }
}