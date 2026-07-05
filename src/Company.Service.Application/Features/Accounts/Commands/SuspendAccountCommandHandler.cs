using Company.Service.Application.Common.Interfaces.Persistence;
using Company.Service.Application.Common.Requests;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Application.Common.Utils;
using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Company.Service.Application.Features.Accounts.Commands;

public record SuspendAccountCommand : ApplicationRequest<Account>
{
    public required Guid Id { get; init; }
}

internal class SuspendAccountCommandHandler : IApplicationRequestHandler<SuspendAccountCommand, Account>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public SuspendAccountCommandHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async ValueTask<ValueResult<Account, ApplicationError>> Handle(SuspendAccountCommand request, CancellationToken cancellationToken)
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

        var hasNonCanceledSubscriptions = await _context.Subscriptions
            .AnyAsync(s => s.AccountId == request.Id && s.Status != SubscriptionStatus.Canceled, cancellationToken);

        if (hasNonCanceledSubscriptions)
        {
            return new BadRequestError()
            {
                Message = "Can't suspend the account because it has non-canceled subscriptions!"
            };
        }

        return await account.Suspend(_timeProvider.GetUtcNowDateTime())
            .TapAsync(async () => await _context.SaveChangesAsync(cancellationToken))
            .MapToValueResultAsync(account)
            .MapErrorAsync(error => new BadRequestError() { Message = error.Message } as ApplicationError);
    }
}