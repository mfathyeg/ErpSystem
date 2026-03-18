using ErpSystem.SharedKernel.Domain;

namespace ErpSystem.Modules.Configuration.Domain.Entities;

public class SystemConfig : AuditableAggregateRoot<Guid>
{
    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsEditable { get; private set; }
    public string? DataType { get; private set; }

    private SystemConfig() { }

    public static SystemConfig Create(
        string key,
        string value,
        string category,
        string description,
        bool isEditable = true,
        string? dataType = "string")
    {
        return new SystemConfig
        {
            Id = Guid.NewGuid(),
            Key = key,
            Value = value,
            Category = category,
            Description = description,
            IsEditable = isEditable,
            DataType = dataType
        };
    }

    public void UpdateValue(string value)
    {
        if (!IsEditable)
            throw new InvalidOperationException("هذا الإعداد غير قابل للتعديل");

        Value = value;
    }

    public void UpdateDescription(string description)
    {
        Description = description;
    }
}
