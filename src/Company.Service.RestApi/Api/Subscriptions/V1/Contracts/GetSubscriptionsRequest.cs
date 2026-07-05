using Company.Service.Application.Features.Subscriptions.Queries;

namespace Company.Service.RestApi.Api.Subscriptions.V1.Contracts;

public record class GetSubscriptionsRequest
{
    public string? SearchTerm { get; init; }

    public ICollection<Guid>? Ids { get; init; }

    public ICollection<BillCycle>? BillCycles { get; init; }

    public ICollection<SubscriptionStatus>? Statuses { get; init; }

    public int PageNumber { get; init; }

    public int PageSize { get; init; }
}

internal static class GetSubscriptionsRequestToQueryMapping
{
    extension(GetSubscriptionsRequest request)
    {
        public GetSubscriptionsQuery ToQuery(Guid accountId) =>
            new()
            {
                AccountId = accountId,
                SearchTerm = request.SearchTerm,
                Ids = request.Ids,
                BillCycles = request.BillCycles?.Select(b => (Domain.Entities.BillCycle)b).ToList() ?? null,
                Statuses = request.Statuses?.Select(s => (Domain.Entities.SubscriptionStatus)s).ToList() ?? null,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
    }
}