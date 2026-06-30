using Company.Service.Application.Features.Accounts.Queries;

namespace Company.Service.RestApi.Api.Accounts.V1.Contracts;

public record class GetAccountsRequest
{
    public string? SearchTerm { get; init; }

    public ICollection<Guid>? TenantIds { get; init; }

    public ICollection<Guid>? Ids { get; init; }

    public ICollection<AccountTier>? AccountTiers { get; init; }

    public ICollection<AccountStatus>? AccountStatuses { get; init; }

    public int PageNumber { get; init; }

    public int PageSize { get; init; }
}

internal static class GetAccountsRequestToQueryMapping
{
    extension(GetAccountsRequest request)
    {
        public GetAccountsQuery ToQuery() =>
            new()
            {
                SearchTerm = request.SearchTerm,
                TenantIds = request.TenantIds,
                Ids = request.Ids,
                AccountTiers = request.AccountTiers?.Select(t => (Domain.Entities.AccountTier)t).ToList() ?? null,
                AccountStatuses = request.AccountStatuses?.Select(s => (Domain.Entities.AccountStatus)s).ToList() ?? null,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
    }
}