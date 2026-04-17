using Company.Service.Application.Common.Interfaces.Persistence;
using Company.Service.Application.Common.Requests;
using Company.Service.Application.Common.Types;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Company.Service.Application.Accounts.Queries;

public record GetAccountOrdersQuery : ApplicationRequest<PagedList<AccountOrder>>, IPagableRequest
{
    public ICollection<Guid>? TenantIds { get; init; }

    public ICollection<Guid>? OrderIds { get; init; }

    public ICollection<Guid>? AccountIds { get; init; }

    public ICollection<AccountOrderStatus>? Statuses { get; init; }

    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}

internal class GetAccountOrdersQueryHandler : IApplicationRequestHandler<GetAccountOrdersQuery, PagedList<AccountOrder>>
{
    private readonly IApplicationDbContext _context;

    public GetAccountOrdersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }


    public async ValueTask<Result<PagedList<AccountOrder>, ApplicationError>> Handle(GetAccountOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AccountOrders.AsNoTracking();

        query = ApplyFilters(query, request);

        var totalCount = query.Count();

        int pageNumber = request.GetPageNumberOrDefault();
        int pageSize = request.GetPageSizeOrDefault();

        var orders = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var pagedList = new PagedList<AccountOrder>(
            items: [.. orders],
            currentPage: pageNumber,
            pageSize: pageSize,
            totalCount: totalCount
        );

        return pagedList;
    }

    private static IQueryable<AccountOrder> ApplyFilters(IQueryable<AccountOrder> query, GetAccountOrdersQuery request)
    {
        if (request.TenantIds is not null && request.TenantIds.Count > 0)
        {
            query = query.Where(o => request.TenantIds.Contains(o.TenantId));
        }

        if (request.OrderIds is not null && request.OrderIds.Count > 0)
        {
            query = query.Where(o => request.OrderIds.Contains(o.Id));
        }

        if (request.AccountIds is not null && request.AccountIds.Count > 0)
        {
            query = query.Where(o => o.AccountId.HasValue && request.AccountIds.Contains(o.AccountId.Value));
        }

        if (request.Statuses is not null && request.Statuses.Count > 0)
        {
            query = query.Where(o => request.Statuses.Contains(o.Status));
        }

        return query;
    }
}