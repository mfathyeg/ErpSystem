using System.Text.RegularExpressions;
using ErpSystem.SharedKernel.Domain;

namespace ErpSystem.Domain.Common.ValueObjects;

public sealed partial class Email : ValueObject
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty.", nameof(value));

        value = value.Trim().ToLowerInvariant();

        if (!EmailRegex().IsMatch(value))
            throw new ArgumentException("Invalid email format.", nameof(value));

        return new Email(value);
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
