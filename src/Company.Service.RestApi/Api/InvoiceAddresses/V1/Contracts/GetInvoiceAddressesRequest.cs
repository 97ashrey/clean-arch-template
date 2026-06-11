using Company.Service.Application.InvoiceAddresses.Queries;

namespace Company.Service.RestApi.Api.InvoiceAddresses.V1.Contracts;

public record class GetInvoiceAddressesRequest
{
    public ICollection<Guid>? TenantIds { get; init; }

    public ICollection<Guid>? InvoiceAddressIds { get; init; }

    public int PageNumber { get; init; }

    public int PageSize { get; init; }
}

public static class GetInvoiceAddressesRequestToQueryMapping
{
    extension(GetInvoiceAddressesRequest request)
    {
        public GetInvoiceAddressesQuery ToQuery() =>
            new ()
        {
            TenantIds = request.TenantIds,
            InvoiceAddressIds = request.InvoiceAddressIds,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}