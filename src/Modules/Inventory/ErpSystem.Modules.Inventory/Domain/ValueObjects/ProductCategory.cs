using ErpSystem.SharedKernel.Domain;

namespace ErpSystem.Modules.Inventory.Domain.ValueObjects;

public sealed class ProductCategory : ValueObject
{
    public string Code { get; }
    public string Name { get; }

    private ProductCategory(string code, string name)
    {
        Code = code;
        Name = name;
    }

    public static ProductCategory Create(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Category code is required.", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name is required.", nameof(name));

        return new ProductCategory(code.ToUpperInvariant(), name);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Code;
        yield return Name;
    }

    public override string ToString() => $"{Code} - {Name}";
}
