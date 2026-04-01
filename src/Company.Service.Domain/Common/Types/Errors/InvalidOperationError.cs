namespace Company.Service.Domain.Common.Types.Errors;

public record class InvalidOperationError(string Message) : DomainError(Message)
{

}
