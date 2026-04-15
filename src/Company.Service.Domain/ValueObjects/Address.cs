using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Common.Types.Errors;
using Company.Service.Domain.Common.Types.Utils;

namespace Company.Service.Domain.ValueObjects;

public record Address
{
    public string Country { get; private set; }

    public string City { get; private set; }

    public string ZipCode {get; private set; } 
    
    public string Street { get; private set; } 

    public string Number { get; private set; }

    private Address(string country, string city, string zipCode, string street, string number)
    {
        Country = country;
        City = city;
        ZipCode = zipCode;
        Street = street;
        Number = number;
    }

    public static Result<Address, ValidationError> CreateNew(string country, string city, string zipCode, string street, string number)
    {
        return Validate.ExecuteRules(
            Validate.NotEmpty(country, nameof(country)),
            Validate.NotEmpty(city, nameof(city)),
            Validate.NotEmpty(zipCode, nameof(zipCode)),
            Validate.NotEmpty(street, nameof(street)),
            Validate.NotEmpty(number, nameof(number))
        ).MapToValueResult(
            new Address(country, city, zipCode, street, number)
        );
    }
}
