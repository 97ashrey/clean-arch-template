using Company.Service.Application.Common.Interfaces.Persistence;
using Company.Service.Application.Common.Requests;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Company.Service.Application.Accounts.Queries;

public record GetAccountOrderByIdQuery : ApplicationRequest<AccountOrder>
{
    public required Guid Id { get; init; }

    public Guid? TenantId { get; init; }
}

internal class GetAccountOrderByIdQueryHandler : IApplicationRequestHandler<GetAccountOrderByIdQuery, AccountOrder>
{
    private readonly IApplicationDbContext _context;

    public GetAccountOrderByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Result<AccountOrder, ApplicationError>> Handle(GetAccountOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AccountOrders
            .AsNoTracking()
            .Where(o => o.Id == request.Id);

        if (request.TenantId.HasValue)
        {
            query = query.Where(o => o.TenantId == request.TenantId);
        }

        var order = await query.FirstOrDefaultAsync(cancellationToken);

        if (order is null)
        {
            return new NotFoundError()
            {
                Message = $"Account order with Id {request.Id} not found."
            };
        }

        return order;
    }
}