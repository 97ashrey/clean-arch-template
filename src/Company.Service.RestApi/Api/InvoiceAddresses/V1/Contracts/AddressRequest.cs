namespace Company.Service.RestApi.Api.InvoiceAddresses.V1.Contracts;

public record class AddressRequest
{
    public required string Street { get; init; }
    public required string City { get; init; }
    public required string ZipCode { get; init; }
    public required string Country { get; init; }
    public required string Number { get; init; }
}
