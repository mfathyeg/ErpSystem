using System.Text.RegularExpressions;
using ErpSystem.SharedKernel.Domain;

namespace ErpSystem.Domain.Common.ValueObjects;

public sealed partial class PhoneNumber : ValueObject
{
    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone number cannot be empty.", nameof(value));

        var cleaned = PhoneCleanRegex().Replace(value, "");

        if (cleaned.Length < 10 || cleaned.Length > 15)
            throw new ArgumentException("Invalid phone number length.", nameof(value));

        return new PhoneNumber(cleaned);
    }

    [GeneratedRegex(@"[\s\-\(\)]+", RegexOptions.Compiled)]
    private static partial Regex PhoneCleanRegex();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(PhoneNumber phone) => phone.Value;
}
