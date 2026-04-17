using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Common.Types.Errors;
using Company.Service.Domain.Common.Types.Utils;
using Company.Service.Domain.Entities;

namespace Company.Service.Domain.ValueObjects;

public record class AccountDetails
{
    public string Name { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public AccountTier Tier { get; private set; }

    public Guid InvoiceAddressId { get; private set; }

    private AccountDetails() { }

    public static Result<AccountDetails, ValidationError> CreateNew(string name, string email, AccountTier tier, Guid invoiceAdressId)
    {
        return Validate.ExecuteRules(
            Validate.NotEmpty(name, nameof(name)),
            Validate.NotEmpty(email, nameof(email)),
            Validate.NotEmpty(invoiceAdressId, nameof(invoiceAdressId))
        ).MapToValueResult(new AccountDetails()
        {
            Name = name,
            Email = email,
            Tier = tier,
            InvoiceAddressId = invoiceAdressId
        });
    }
}