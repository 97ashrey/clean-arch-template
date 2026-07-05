using Company.Service.Application.Common.Interfaces.Persistence;
using Company.Service.Application.Common.Requests;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Company.Service.Application.Features.Subscriptions.Queries;

public record GetSubscriptionByIdQuery : ApplicationRequest<Subscription>
{
    public required Guid AccountId { get; init; }
    public required Guid Id { get; init; }
}

internal class GetSubscriptionByIdQueryHandler : IApplicationRequestHandler<GetSubscriptionByIdQuery, Subscription>
{
    private readonly IApplicationDbContext _context;

    public GetSubscriptionByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<ValueResult<Subscription, ApplicationError>> Handle(GetSubscriptionByIdQuery request, CancellationToken cancellationToken)
    {
        var subscription = await _context.Subscriptions
            .AsNoTracking()
            .Where(s => s.Id == request.Id && s.AccountId == request.AccountId)
            .FirstOrDefaultAsync(cancellationToken);

        if (subscription is null)
        {
            return new NotFoundError()
            {
                Message = $"Subscription with Id {request.Id} not found."
            };
        }

        return subscription;
    }
}