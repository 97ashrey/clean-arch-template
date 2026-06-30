using Company.Service.Application.Features.InvoiceAddresses.Commands;

namespace Company.Service.RestApi.Api.InvoiceAddresses.V1.Contracts;

public record class UpdateInvoiceAddressRequest
{
    public required string Name { get; init; }

    public required AddressRequest Address { get; init; }
}

internal static class UpdateInvoiceAddressRequestToCommandMapping
{
    extension(UpdateInvoiceAddressRequest request)
    {
        public UpdateInvoiceAddressCommand ToCommand(Guid id) =>
            new()
            {
                Id = id,
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