using Company.Service.Application.Features.Subscriptions.Commands;

namespace Company.Service.RestApi.Api.Subscriptions.V1.Contracts;

public record class UpdateSubscriptionRequest
{
    public required string FriendlyName { get; init; }
}

internal static class UpdateSubscriptionRequestToCommandMapping
{
    extension(UpdateSubscriptionRequest request)
    {
        public UpdateSubscriptionCommand ToCommand(Guid accountId, Guid id) =>
            new()
            {
                AccountId = accountId,
                Id = id,
                FriendlyName = request.FriendlyName
            };
    }
}