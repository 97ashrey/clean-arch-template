namespace Company.Service.Domain.Common.Types.Errors;

public record class ValidationError(string Message, ValidationFailure[] Failures) : DomainError(Message);

public record ValidationFailure(string PropertyName, string[] Errors);