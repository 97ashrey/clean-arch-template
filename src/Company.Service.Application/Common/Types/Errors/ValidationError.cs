namespace Company.Service.Application.Common.Types.Errors;

public record ValidationError : ApplicationError
{
    public required ValidationFailure[] Failures { get; init; }

    public override string ToString()
    {
        var propertyFormattedErrors = new List<string>();
        foreach (var (property, propertyErrors) in Failures)
        {
            // Property: [error1, error2, error3]
            propertyFormattedErrors.Add($"{property}: [{string.Join(", ", propertyErrors)}]");
        }
        // { Property1: [error1, error2], Property2: [error1, error2].... }
        var formattedErrors = $"{{ {string.Join(", ", propertyFormattedErrors)} }}";


        return $"{nameof(ValidationError)} {{ {nameof(Id)} = {Id}, {nameof(Message)} = {Message}, {nameof(Failures)} = {formattedErrors} }}";
    }
}

public record ValidationFailure(string PropertyName, string[] Errors);

internal static class ValidationErrorExtensions
{
    extension(Domain.Common.Types.Errors.ValidationError domainError)
    {
        public ValidationError ToAppValidationError()
        {
            return new ValidationError()
            {
                Message = domainError.Message,
                Failures = domainError.Failures
                    .Select(f => new ValidationFailure(f.PropertyName, f.Errors))
                    .ToArray()
            };
        }
    }
}