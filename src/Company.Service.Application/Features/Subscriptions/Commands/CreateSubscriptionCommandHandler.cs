using Company.Service.Application.Common.Interfaces.Persistence;
using Company.Service.Application.Common.Requests;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Application.Common.Utils;
using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;
using FluentValidation;
using MassTransit;

namespace Company.Service.Application.Features.Subscriptions.Commands;

public record CreateSubscriptionCommand : ApplicationRequest<Subscription>
{
    public required Guid AccountId { get; init; }
    public required string Name { get; init; }
    public required string FriendlyName { get; init; }
    public required PriceCommand PurchasePrice { get; init; }
    public required BillCycle BillCycle { get; init; }
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }
    public required Guid ProductId { get; init; }

    public record PriceCommand
    {
        public required decimal Value { get; init; }
        public required string Currency { get; init; }
    }
}

internal class CreateSubscriptionCommandValidator : AbstractValidator<CreateSubscriptionCommand>
{
    public CreateSubscriptionCommandValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.FriendlyName).NotEmpty();
        RuleFor(x => x.PurchasePrice.Value).GreaterThan(0);
        RuleFor(x => x.PurchasePrice.Currency).NotEmpty().Length(3);
        RuleFor(x => x.BillCycle).IsInEnum();
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
    }
}

internal class CreateSubscriptionCommandHandler : IApplicationRequestHandler<CreateSubscriptionCommand, Subscription>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly TimeProvider _timeProvider;

    public CreateSubscriptionCommandHandler(IApplicationDbContext dbContext, IPublishEndpoint publishEndpoint, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _timeProvider = timeProvider;
    }

    public async ValueTask<ValueResult<Subscription, ApplicationError>> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        return await Price
            .CreateNew(request.PurchasePrice.Value, request.PurchasePrice.Currency)
            .Bind(price => Subscription.CreateNew(
                accountId: request.AccountId,
                name: request.Name,
                friendlyName: request.FriendlyName,
                purchasePrice: price,
                billCycle: request.BillCycle,
                startDate: request.StartDate,
                endDate: request.EndDate,
                productId: request.ProductId))
            .MapError<ApplicationError>(error => error.ToAppValidationError())
            .TapAsync(async subscription =>
            {
                _dbContext.Subscriptions.Add(subscription);

                await _publishEndpoint.Publish(CreateSubscriptionCreatedEvent(subscription));

                await _dbContext.SaveChangesAsync(cancellationToken);
            });
    }

    private IntegrationEvents.V1.Subscriptions.SubscriptionCreatedEvent CreateSubscriptionCreatedEvent(Subscription subscription)
    {
        return new(
            SubscriptionId: subscription.Id,
            AccountId: subscription.AccountId,
            Name: subscription.Name,
            FriendlyName: subscription.FriendlyName,
            PurchasePrice: new IntegrationEvents.V1.Shared.Price(
                subscription.PurchasePrice.Value,
                subscription.PurchasePrice.Currency
            ),
            BillCycle: (IntegrationEvents.V1.Shared.BillCycle)subscription.BillCycle,
            StartDate: subscription.StartDate,
            EndDate: subscription.EndDate,
            ProductId: subscription.ProductId,
            CreatedDate: _timeProvider.GetUtcNowDateTime()
        );
    }
}