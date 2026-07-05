namespace Company.Service.RestApi.Api.Subscriptions.V1.Contracts;

public record Price
{
    public required decimal Value { get; init; }
    public required string Currency { get; init; }
}