using Company.Service.Application.Common.Interfaces.Persistence;
using Company.Service.Application.Common.Requests;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Company.Service.Application.Features.Accounts.Commands;

public record UpdateAccountCommand : ApplicationRequest<Account>
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }
}

internal class UpdateAccountCommandHandler : IApplicationRequestHandler<UpdateAccountCommand, Account>
{
    private readonly IApplicationDbContext _context;

    public UpdateAccountCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<ValueResult<Account, ApplicationError>> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
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

        return await account.ChangeName(request.Name)
            .TapAsync(async () => await _context.SaveChangesAsync(cancellationToken))
            .MapToValueResultAsync(account)
            .MapErrorAsync(error => error.ToAppValidationError() as ApplicationError);
    }
}