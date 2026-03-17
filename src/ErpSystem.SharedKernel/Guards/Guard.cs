using System.Runtime.CompilerServices;

namespace ErpSystem.SharedKernel.Guards;

public static class Guard
{
    public static T AgainstNull<T>(
        T? value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) where T : class
    {
        if (value is null)
            throw new ArgumentNullException(parameterName);

        return value;
    }

    public static string AgainstNullOrEmpty(
        string? value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Value cannot be null or empty.", parameterName);

        return value;
    }

    public static string AgainstNullOrWhiteSpace(
        string? value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null or whitespace.", parameterName);

        return value;
    }

    public static T AgainstOutOfRange<T>(
        T value,
        T min,
        T max,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
            throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be between {min} and {max}.");

        return value;
    }

    public static int AgainstNegative(
        int value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(parameterName, value, "Value cannot be negative.");

        return value;
    }

    public static decimal AgainstNegative(
        decimal value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(parameterName, value, "Value cannot be negative.");

        return value;
    }

    public static Guid AgainstEmpty(
        Guid value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Guid cannot be empty.", parameterName);

        return value;
    }

    public static IEnumerable<T> AgainstNullOrEmptyCollection<T>(
        IEnumerable<T>? value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value is null || !value.Any())
            throw new ArgumentException("Collection cannot be null or empty.", parameterName);

        return value;
    }

    public static string AgainstLengthExceeded(
        string value,
        int maxLength,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value.Length > maxLength)
            throw new ArgumentException($"String length cannot exceed {maxLength} characters.", parameterName);

        return value;
    }
}
