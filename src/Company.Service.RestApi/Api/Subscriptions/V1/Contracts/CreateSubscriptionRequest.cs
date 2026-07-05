using Company.Service.Application.Features.Subscriptions.Commands;

namespace Company.Service.RestApi.Api.Subscriptions.V1.Contracts;

public record class CreateSubscriptionRequest
{
    public required string Name { get; init; }
    public required string FriendlyName { get; init; }
    public required Price PurchasePrice { get; init; }
    public required BillCycle BillCycle { get; init; }
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }
    public required Guid ProductId { get; init; }
}

internal static class CreateSubscriptionRequestToCommandMapping
{
    extension(CreateSubscriptionRequest request)
    {
        public CreateSubscriptionCommand ToCommand(Guid accountId) =>
            new()
            {
                AccountId = accountId,
                Name = request.Name,
                FriendlyName = request.FriendlyName,
                PurchasePrice = new CreateSubscriptionCommand.PriceCommand { Value = request.PurchasePrice.Value, Currency = request.PurchasePrice.Currency },
                BillCycle = (Domain.Entities.BillCycle)request.BillCycle,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                ProductId = request.ProductId
            };
    }
}