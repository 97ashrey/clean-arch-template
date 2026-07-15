using Company.Service.Application.Common.Interfaces.Persistence;
using Company.Service.Application.Common.Requests;
using Company.Service.Application.Common.Types;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Company.Service.Application.Features.Accounts.Queries;

public record GetAccountsQuery : ApplicationRequest<PagedList<Account>>, IPagableRequest
{
    public string? SearchTerm { get; init; }

    public ICollection<Guid>? TenantIds { get; init; }

    public ICollection<Guid>? Ids { get; init; }

    public ICollection<AccountTier>? AccountTiers { get; init; }

    public ICollection<AccountStatus>? AccountStatuses { get; init; }

    public int PageNumber { get; init; }

    public int PageSize { get; init; }
}

internal class GetAccountsQueryHandler : IApplicationRequestHandler<GetAccountsQuery, PagedList<Account>>
{
    private readonly IApplicationDbContext _context;

    public GetAccountsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<ValueResult<PagedList<Account>, ApplicationError>> Handle(GetAccountsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Accounts.AsNoTracking();

        query = ApplyFilters(query, request);

        var totalCount = await query.CountAsync(cancellationToken);

        int pageNumber = request.GetPageNumberOrDefault();
        int pageSize = request.GetPageSizeOrDefault();

        var accounts = await query
            .OrderBy(a => a.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var pagedList = new PagedList<Account>(
            items: [.. accounts],
            currentPage: pageNumber,
            pageSize: pageSize,
            totalCount: totalCount
        );

        return pagedList;
    }

    private static IQueryable<Account> ApplyFilters(IQueryable<Account> query, GetAccountsQuery request)
    {
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim().ToLower();
            query = query.Where(a =>
                a.Name.ToLower().Contains(search) ||
                a.Email.ToLower().Contains(search));
        }

        if (request.TenantIds is not null && request.TenantIds.Count > 0)
        {
            query = query.Where(a => request.TenantIds.Contains(a.TenantId));
        }

        if (request.Ids is not null && request.Ids.Count > 0)
        {
            query = query.Where(a => request.Ids.Contains(a.Id));
        }

        if (request.AccountTiers is not null && request.AccountTiers.Count > 0)
        {
            query = query.Where(a => request.AccountTiers.Contains(a.Tier));
        }

        if (request.AccountStatuses is not null && request.AccountStatuses.Count > 0)
        {
            query = query.Where(a => request.AccountStatuses.Contains(a.Status));
        }

        return query;
    }
}