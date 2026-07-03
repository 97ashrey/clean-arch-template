//__EXAMPLE_START__
namespace Company.Service.RestApi.Api.InvoiceAddresses.V1.Contracts;

public record class InvoiceAddress
{
    public required Guid Id { get; init; }

    public required Guid TenantId { get; init; }

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

internal static class InvoiceAddressesV1Mapping
{
    extension(Domain.Entities.InvoiceAddress invoiceAddress)
    {
        public InvoiceAddress ToV1() =>
            new()
            {
                Id = invoiceAddress.Id,
                TenantId = invoiceAddress.TenantId,
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
//__EXAMPLE_END__
