namespace Company.Service.Domain.Common;

public static class Guard
{
    public static void AgainstNullOrEmpty(string? value, string paramName)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException($"{paramName} must not be empty!", paramName);
        }
    }

    public static void AgainstZero(decimal value, string paramName)
    {
        if (value == 0)
        {
            throw new ArgumentException($"{paramName} must not be 0!", paramName);
        }
    }


    public static void AgainstZero(float value, string paramName)
    {
        if (value == 0)
        {
            throw new ArgumentException($"{paramName} must not be 0!", paramName);
        }
    }

    public static void AgainstZero(double value, string paramName)
    {
        if (value == 0)
        {
            throw new ArgumentException($"{paramName} must not be 0!", paramName);
        }
    }

    public static void AgainstZero(int value, string paramName)
    {
        if (value == 0)
        {
            throw new ArgumentException($"{paramName} must not be 0!", paramName);
        }
    }
}
