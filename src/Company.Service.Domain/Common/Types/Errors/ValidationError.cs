namespace Company.Service.Domain.Common.Types.Errors;

public record class ValidationError(string Message, ValidationFailure[] Failures) : DomainError(Message)
{
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


        return $"{nameof(ValidationError)} {{ {nameof(Message)} = {Message}, {nameof(Failures)} = {formattedErrors} }}";
    }
}

public record ValidationFailure(string PropertyName, string[] Errors);
