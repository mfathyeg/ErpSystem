using ErpSystem.SharedKernel.Domain;

namespace ErpSystem.Modules.Notifications.Domain.ValueObjects;

public sealed class NotificationType : ValueObject
{
    public static NotificationType Info => new("Info", "معلومة");
    public static NotificationType Success => new("Success", "نجاح");
    public static NotificationType Warning => new("Warning", "تحذير");
    public static NotificationType Error => new("Error", "خطأ");
    public static NotificationType Order => new("Order", "طلب");
    public static NotificationType Inventory => new("Inventory", "مخزون");
    public static NotificationType System => new("System", "نظام");

    public string Code { get; }
    public string Name { get; }

    private NotificationType(string code, string name)
    {
        Code = code;
        Name = name;
    }

    public static NotificationType Create(string code)
    {
        return code.ToLower() switch
        {
            "info" => Info,
            "success" => Success,
            "warning" => Warning,
            "error" => Error,
            "order" => Order,
            "inventory" => Inventory,
            "system" => System,
            _ => new NotificationType(code, code)
        };
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }
}
