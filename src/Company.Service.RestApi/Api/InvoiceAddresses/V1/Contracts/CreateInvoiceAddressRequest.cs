using Company.Service.Application.InvoiceAddresses.Commands;

namespace Company.Service.RestApi.Api.InvoiceAddresses.V1.Contracts;

public record class CreateInvoiceAddressRequest
{
    public required Guid TenantId { get; init; }

    public required string Name { get; init; }

    public required AddressRequest Address { get; init; }

    public record AddressRequest
    {
        public required string Street { get; init; }
        public required string City { get; init; }
        public required string ZipCode { get; init; }
        public required string Country { get; init; }
        public required string Number { get; init; }
    }
}

internal static class CreateInvoiceAddressRequestToCommandMapping
{
    extension(CreateInvoiceAddressRequest request)
    {
        public CreateInvoiceAddressCommand ToCommand() =>
            new()
            {
                TenantId = request.TenantId,
                Name = request.Name,
                Address = new()
                {
                    Street = request.Address.Street,
                    City = request.Address.City,
                    ZipCode = request.Address.ZipCode,
                    Country = request.Address.Country,
                    Number = request.Address.Number
                }
            };
    }
}