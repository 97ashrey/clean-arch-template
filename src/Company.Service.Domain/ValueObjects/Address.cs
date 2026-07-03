//__EXAMPLE_START__
using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Common.Types.Errors;
using Company.Service.Domain.Common.Utils;

namespace Company.Service.Domain.ValueObjects;

public record Address
{
    public string Country { get; private set; } = string.Empty;

    public string City { get; private set; } = string.Empty;

    public string ZipCode { get; private set; } = string.Empty;

    public string Street { get; private set; } = string.Empty;

    public string Number { get; private set; } = string.Empty;

    private Address() { }

    public static ValueResult<Address, ValidationError> CreateNew(string country, string city, string zipCode, string street, string number)
    {
        return Validate.ExecuteRules(
            Validate.NotEmpty(country, nameof(country)),
            Validate.NotEmpty(city, nameof(city)),
            Validate.NotEmpty(zipCode, nameof(zipCode)),
            Validate.NotEmpty(street, nameof(street)),
            Validate.NotEmpty(number, nameof(number))
        ).MapToValueResult(
            new Address()
            {
                Country = country,
                City = city,
                ZipCode = zipCode,
                Street = street,
                Number = number
            }
        );
    }
}
//__EXAMPLE_END__
