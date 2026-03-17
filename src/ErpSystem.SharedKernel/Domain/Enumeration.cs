using System.Reflection;

namespace ErpSystem.SharedKernel.Domain;

public abstract class Enumeration<TEnum> : IEquatable<Enumeration<TEnum>>
    where TEnum : Enumeration<TEnum>
{
    private static readonly Dictionary<int, TEnum> Enumerations = CreateEnumerations();

    public int Id { get; protected init; }
    public string Name { get; protected init; } = string.Empty;

    protected Enumeration(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public static TEnum? FromId(int id)
    {
        return Enumerations.TryGetValue(id, out var enumeration) ? enumeration : null;
    }

    public static TEnum? FromName(string name)
    {
        return Enumerations.Values.SingleOrDefault(e =>
            e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public static IReadOnlyCollection<TEnum> GetAll()
    {
        return Enumerations.Values.ToList().AsReadOnly();
    }

    public bool Equals(Enumeration<TEnum>? other)
    {
        if (other is null) return false;
        return GetType() == other.GetType() && Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is Enumeration<TEnum> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override string ToString()
    {
        return Name;
    }

    private static Dictionary<int, TEnum> CreateEnumerations()
    {
        var enumerationType = typeof(TEnum);

        var fieldsForType = enumerationType
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(fieldInfo => enumerationType.IsAssignableFrom(fieldInfo.FieldType))
            .Select(fieldInfo => (TEnum)fieldInfo.GetValue(default)!);

        return fieldsForType.ToDictionary(x => x.Id);
    }
}
