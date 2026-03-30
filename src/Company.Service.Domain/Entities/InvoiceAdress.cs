using Company.Service.Domain.Common;

namespace Company.Service.Domain.Entities;

public class InvoiceAdress
{
    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Country { get; private set; } = string.Empty;

    public string City { get; private set; } = string.Empty;

    public string ZipCode {get; private set; } = string.Empty;
    
    public string Street { get; private set; } = string.Empty;

    public string Number { get; private set; } = string.Empty;

    internal InvoiceAdress(Guid id, Guid tenantId, string name, string country, string city, string zipCode, string street, string number)
    {
        Id = id;
        Name = name;
        Country = country;
        City = city;
        ZipCode = zipCode;
        Street = street;
        Number = number;
        TenantId = tenantId;
    }

    public void Update(string name, string country, string city, string zipCode, string street, string number)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNullOrEmpty(country, nameof(country));
        Guard.AgainstNullOrEmpty(city, nameof(city));
        Guard.AgainstNullOrEmpty(zipCode, nameof(zipCode));
        Guard.AgainstNullOrEmpty(street, nameof(street));
        Guard.AgainstNullOrEmpty(number, nameof(number));

        Name = name;
        Country = country;
        City = city;
        ZipCode = zipCode;
        Street = street;
        Number = number;
    }

    public string GetFormattedAddress()
    {
        return $"{Street} {Number}, {Country} {ZipCode} {City}";
    }
}

public static class InvoiceAdressConstruction
{
    extension(InvoiceAdress)
    {
        public static InvoiceAdress CreateNew(Guid tenantId, string name, string country, string city, string zipCode, string street, string number)
        {
            Guard.AgainstNullOrEmpty(name, nameof(name));
            Guard.AgainstNullOrEmpty(country, nameof(country));
            Guard.AgainstNullOrEmpty(city, nameof(city));
            Guard.AgainstNullOrEmpty(zipCode, nameof(zipCode));
            Guard.AgainstNullOrEmpty(street, nameof(street));
            Guard.AgainstNullOrEmpty(number, nameof(number));

            return new InvoiceAdress(id: Guid.NewGuid(), tenantId, name, country, city, zipCode, street, number);
        }
    }
}
