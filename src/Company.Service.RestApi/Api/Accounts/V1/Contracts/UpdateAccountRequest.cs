using Company.Service.Application.Features.Accounts.Commands;

namespace Company.Service.RestApi.Api.Accounts.V1.Contracts;

public record class UpdateAccountRequest
{
    public required string Name { get; init; }
}

internal static class UpdateAccountRequestToCommandMapping
{
    extension(UpdateAccountRequest request)
    {
        public UpdateAccountCommand ToCommand(Guid id) =>
            new()
            {
                Id = id,
                Name = request.Name
            };
    }
}