namespace Company.Service.Application.Common.Types.Errors;

public record ApplicationError
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Message { get; init; }
}