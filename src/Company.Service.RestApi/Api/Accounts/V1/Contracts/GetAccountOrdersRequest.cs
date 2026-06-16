using Company.Service.Application.Features.Accounts.Queries;
using Company.Service.Domain.Entities;

namespace Company.Service.RestApi.Api.Accounts.V1.Contracts;

public record class GetAccountOrdersRequest
{
    public ICollection<Guid>? TenantIds { get; init; }

    public ICollection<Guid>? OrderIds { get; init; }

    public ICollection<Guid>? AccountIds { get; init; }

    public ICollection<AccountOrderStatus>? Statuses { get; init; }

    public int PageNumber { get; init; }

    public int PageSize { get; init; }
}

internal static class GetAccountOrdersRequestToQueryMapping
{
    extension(GetAccountOrdersRequest request)
    {
        public GetAccountOrdersQuery ToQuery() =>
            new()
            {
                TenantIds = request.TenantIds,
                OrderIds = request.OrderIds,
                AccountIds = request.AccountIds,
                Statuses = request.Statuses?.Select(status => (Domain.Entities.AccountOrderStatus)status).ToList() ?? null,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
    }
}