using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Common.Types.Errors;
using Company.Service.Domain.Common.Utils;

namespace Company.Service.Domain.ValueObjects;

public record Price(decimal Value, string Currency)
{
    private static readonly string[] ValidCurrencies =
    [
        "EUR", "GBP", "CHF", "NOK", "SEK", "DKK",
        "PLN", "CZK", "HUF", "RON", "BGN", "ISK",
        "USD", "CAD"
    ];

    public static ValueResult<Price, ValidationError> CreateNew(decimal value, string currency)
    {
        return Validate.ExecuteRules(
            Validate.NotZero(value, nameof(value)),
            Validate.NotEmpty(currency, nameof(currency)),
            Validate.Must(() =>
            {
                if (!ValidCurrencies.Contains(currency, StringComparer.OrdinalIgnoreCase))
                {
                    return new SingleFailure(nameof(currency), $"Must be one of: {string.Join(", ", ValidCurrencies)}!");
                }

                return null;
            })
        ).MapToValueResult(new Price(value, currency.ToUpperInvariant()));
    }
}