using Company.Service.Application.Common.Interfaces.Persistence;
using Company.Service.Application.Common.Requests;
using Company.Service.Application.Common.Types;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Company.Service.Application.Features.Subscriptions.Queries;

public record GetSubscriptionsQuery : ApplicationRequest<PagedList<Subscription>>, IPagableRequest
{
    public required Guid AccountId { get; init; }

    public string? SearchTerm { get; init; }

    public ICollection<Guid>? Ids { get; init; }

    public ICollection<BillCycle>? BillCycles { get; init; }

    public ICollection<SubscriptionStatus>? Statuses { get; init; }

    public int PageNumber { get; init; }

    public int PageSize { get; init; }
}

internal class GetSubscriptionsQueryHandler : IApplicationRequestHandler<GetSubscriptionsQuery, PagedList<Subscription>>
{
    private readonly IApplicationDbContext _context;

    public GetSubscriptionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<ValueResult<PagedList<Subscription>, ApplicationError>> Handle(GetSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Subscriptions.AsNoTracking();

        query = ApplyFilters(query, request);

        var totalCount = await query.CountAsync(cancellationToken);

        int pageNumber = request.GetPageNumberOrDefault();
        int pageSize = request.GetPageSizeOrDefault();

        var subscriptions = await query
            .OrderBy(s => s.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var pagedList = new PagedList<Subscription>(
            items: [.. subscriptions],
            currentPage: pageNumber,
            pageSize: pageSize,
            totalCount: totalCount
        );

        return pagedList;
    }

    private static IQueryable<Subscription> ApplyFilters(IQueryable<Subscription> query, GetSubscriptionsQuery request)
    {
        query = query.Where(s => s.AccountId == request.AccountId);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim().ToLower();
            query = query.Where(s =>
                s.Name.ToLower().Contains(search) ||
                s.FriendlyName.ToLower().Contains(search));
        }

        if (request.Ids is not null && request.Ids.Count > 0)
        {
            query = query.Where(s => request.Ids.Contains(s.Id));
        }

        if (request.BillCycles is not null && request.BillCycles.Count > 0)
        {
            query = query.Where(s => request.BillCycles.Contains(s.BillCycle));
        }

        if (request.Statuses is not null && request.Statuses.Count > 0)
        {
            query = query.Where(s => request.Statuses.Contains(s.Status));
        }

        return query;
    }
}