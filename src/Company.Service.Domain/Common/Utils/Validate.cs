using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Common.Types.Errors;

namespace Company.Service.Domain.Common.Utils;

public record SingleFailure(string PropertyName, string Error);

public static class Validate
{
    public static Result<ValidationError> ExecuteRules(params IEnumerable<SingleFailure?> rules)
    {
        var singleFailures = new List<SingleFailure>();

        foreach (var failedRule in rules)
        {
            if (failedRule is not null)
            {
                singleFailures.Add(failedRule);
            }
        }

        if (singleFailures.Count == 0)
        {
            return new();
        }

        var groupedFailures = singleFailures.GroupBy(f => f.PropertyName).Select(g => new ValidationFailure(g.Key, [.. g.Select(x => x.Error)]));

        return new ValidationError("Validation failed!", [.. groupedFailures]);
    }

    public static SingleFailure? Must(Func<SingleFailure?> rule)
    {
        return rule();
    }

    public static SingleFailure? NotEmpty(string value, string propertyName)
    {
        if (string.IsNullOrEmpty(value))
        {
            return new(propertyName, "Must not be empty!");
        }

        return null;
    }

    public static SingleFailure? NotEmpty(Guid value, string propertyName)
    {
        if (value == Guid.Empty)
        {
            return new(propertyName, "Must not be empty!");
        }

        return null;
    }

    public static SingleFailure? NotZero(int value, string propertyName)
    {
        if (value == 0)
        {
            return new(propertyName, "Must not be zero!");
        }

        return null;
    }

    public static SingleFailure? NotZero(decimal value, string propertyName)
    {
        if (value == 0)
        {
            return new(propertyName, "Must not be zero!");
        }

        return null;
    }

    public static SingleFailure? NotZero(float value, string propertyName)
    {
        if (value == 0)
        {
            return new(propertyName, "Must not be zero!");
        }

        return null;
    }

    public static SingleFailure? NotZero(double value, string propertyName)
    {
        if (value == 0)
        {
            return new(propertyName, "Must not be zero!");
        }

        return null;
    }
}