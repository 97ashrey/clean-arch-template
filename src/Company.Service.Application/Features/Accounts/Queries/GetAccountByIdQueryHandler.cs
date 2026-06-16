using Company.Service.Application.Common.Interfaces.Persistence;
using Company.Service.Application.Common.Requests;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Company.Service.Application.Features.Accounts.Queries;

public record GetAccountByIdQuery : ApplicationRequest<Account>
{
    public required Guid Id { get; init; }

    public Guid? TenantId { get; init; }
}

internal class GetAccountByIdQueryHandler : IApplicationRequestHandler<GetAccountByIdQuery, Account>
{
    private readonly IApplicationDbContext _context;

    public GetAccountByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<ValueResult<Account, ApplicationError>> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Accounts
            .AsNoTracking()
            .Where(a => a.Id == request.Id);

        if (request.TenantId.HasValue)
        {
            query = query.Where(a => a.TenantId == request.TenantId);
        }

        var account = await query.FirstOrDefaultAsync(cancellationToken);

        if (account is null)
        {
            return new NotFoundError()
            {
                Message = $"Account with Id {request.Id} not found."
            };
        }

        return account;
    }
}