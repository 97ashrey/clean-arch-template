using Company.Service.Application.Common.Interfaces.Persistence;
using Company.Service.Application.Common.Requests;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Company.Service.Application.Features.Accounts.Commands;

public record RemoveAccountCommand : ApplicationRequest<Account>
{
    public required Guid Id { get; init; }
}

internal class RemoveAccountCommandHandler : IApplicationRequestHandler<RemoveAccountCommand, Account>
{
    private readonly IApplicationDbContext _context;

    public RemoveAccountCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<ValueResult<Account, ApplicationError>> Handle(RemoveAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (account is null)
        {
            return new NotFoundError()
            {
                Message = $"Account with Id {request.Id} not found."
            };
        }

        return await account.Remove()
            .TapAsync(async () => await _context.SaveChangesAsync(cancellationToken))
            .MapToValueResultAsync(account)
            .MapErrorAsync(error => new BadRequestError() { Message = error.Message } as ApplicationError);
    }
}