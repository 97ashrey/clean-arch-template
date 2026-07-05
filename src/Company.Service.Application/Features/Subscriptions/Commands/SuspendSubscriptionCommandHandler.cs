using Company.Service.Application.Common.Interfaces.Persistence;
using Company.Service.Application.Common.Requests;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Application.Common.Utils;
using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Company.Service.Application.Features.Subscriptions.Commands;

public record SuspendSubscriptionCommand : ApplicationRequest<Subscription>
{
    public required Guid AccountId { get; init; }
    public required Guid Id { get; init; }
}

internal class SuspendSubscriptionCommandHandler : IApplicationRequestHandler<SuspendSubscriptionCommand, Subscription>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public SuspendSubscriptionCommandHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async ValueTask<ValueResult<Subscription, ApplicationError>> Handle(SuspendSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.AccountId == request.AccountId && s.Id == request.Id, cancellationToken);

        if (subscription is null)
        {
            return new NotFoundError()
            {
                Message = $"Subscription with Id {request.Id} not found."
            };
        }

        return await subscription.Suspend(_timeProvider.GetUtcNowDateTime())
            .TapAsync(async () => await _context.SaveChangesAsync(cancellationToken))
            .MapToValueResultAsync(subscription)
            .MapErrorAsync(error => new BadRequestError() { Message = error.Message } as ApplicationError);
    }
}