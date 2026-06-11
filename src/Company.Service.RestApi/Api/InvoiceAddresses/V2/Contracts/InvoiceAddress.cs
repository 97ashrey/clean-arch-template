namespace Company.Service.RestApi.Api.InvoiceAddresses.V2.Contracts;

public record class InvoiceAddress
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required Address Address { get; init; }
}

public record Address
{
    public required string Country { get; init; }

    public required string City { get; init; }

    public required string ZipCode { get; init; }

    public required string Street { get; init; }

    public required string Number { get; init; }
}

internal static class InvoiceAddressesV2Mapping
{
    extension(Domain.Entities.InvoiceAddress invoiceAddress)
    {
        public InvoiceAddress ToV2() =>
            new ()
            {
                Id = invoiceAddress.Id,
                Name = invoiceAddress.Name,
                Address = new()
                {
                    Street = invoiceAddress.Address.Street,
                    City = invoiceAddress.Address.City,
                    ZipCode = invoiceAddress.Address.ZipCode,
                    Country = invoiceAddress.Address.Country,
                    Number = invoiceAddress.Address.Number
                }
            };
    }
}