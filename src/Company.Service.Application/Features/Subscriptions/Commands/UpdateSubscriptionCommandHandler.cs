using Company.Service.Application.Common.Interfaces.Persistence;
using Company.Service.Application.Common.Requests;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Company.Service.Application.Features.Subscriptions.Commands;

public record UpdateSubscriptionCommand : ApplicationRequest<Subscription>
{
    public required Guid AccountId { get; init; }
    public required Guid Id { get; init; }
    public required string FriendlyName { get; init; }
}

internal class UpdateSubscriptionCommandValidator : AbstractValidator<UpdateSubscriptionCommand>
{
    public UpdateSubscriptionCommandValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FriendlyName).NotEmpty();
    }
}

internal class UpdateSubscriptionCommandHandler : IApplicationRequestHandler<UpdateSubscriptionCommand, Subscription>
{
    private readonly IApplicationDbContext _context;

    public UpdateSubscriptionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<ValueResult<Subscription, ApplicationError>> Handle(UpdateSubscriptionCommand request, CancellationToken cancellationToken)
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

        return await subscription.Update(request.FriendlyName)
            .TapAsync(async () => await _context.SaveChangesAsync(cancellationToken))
            .MapToValueResultAsync(subscription)
            .MapErrorAsync(error => error.ToAppValidationError() as ApplicationError);
    }
}