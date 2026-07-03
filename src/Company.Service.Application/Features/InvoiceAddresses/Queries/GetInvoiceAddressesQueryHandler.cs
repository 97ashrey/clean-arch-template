//__EXAMPLE_START__
using Company.Service.Application.Common.Interfaces.Persistence;
using Company.Service.Application.Common.Requests;
using Company.Service.Application.Common.Types;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Company.Service.Application.Features.InvoiceAddresses.Queries;

public record GetInvoiceAddressesQuery : ApplicationRequest<PagedList<InvoiceAddress>>, IPagableRequest
{
    public ICollection<Guid>? TenantIds { get; init; }

    public ICollection<Guid>? InvoiceAddressIds { get; init; }

    public int PageNumber { get; init; }

    public int PageSize { get; init; }
}

internal class GetInvoiceAddressesQueryHandler : IApplicationRequestHandler<GetInvoiceAddressesQuery, PagedList<InvoiceAddress>>
{
    private readonly IApplicationDbContext _context;

    public GetInvoiceAddressesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<ValueResult<PagedList<InvoiceAddress>, ApplicationError>> Handle(GetInvoiceAddressesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.InvoiceAdresses.AsNoTracking();

        query = ApplyFilters(query, request);

        var totalCount = query.Count();

        int pageNumber = request.GetPageNumberOrDefault();
        int pageSize = request.GetPageSizeOrDefault();

        var addresses = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var pagedList = new PagedList<InvoiceAddress>(
            items: [.. addresses],
            currentPage: pageNumber,
            pageSize: pageSize,
            totalCount: totalCount
        );

        return pagedList;
    }

    private static IQueryable<InvoiceAddress> ApplyFilters(IQueryable<InvoiceAddress> query, GetInvoiceAddressesQuery request)
    {
        if (request.TenantIds is not null && request.TenantIds.Count > 0)
        {
            query = query.Where(a => request.TenantIds.Contains(a.TenantId));
        }

        if (request.InvoiceAddressIds is not null && request.InvoiceAddressIds.Count > 0)
        {
            query = query.Where(a => request.InvoiceAddressIds.Contains(a.Id));
        }

        return query;
    }
}
//__EXAMPLE_END__