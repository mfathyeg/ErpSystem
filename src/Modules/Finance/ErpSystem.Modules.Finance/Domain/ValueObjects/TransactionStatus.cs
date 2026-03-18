using ErpSystem.SharedKernel.Domain;

namespace ErpSystem.Modules.Finance.Domain.ValueObjects;

public sealed class TransactionStatus : ValueObject
{
    public static TransactionStatus Pending => new("Pending", "معلق");
    public static TransactionStatus Completed => new("Completed", "مكتمل");
    public static TransactionStatus Cancelled => new("Cancelled", "ملغي");

    public string Code { get; }
    public string Name { get; }

    private TransactionStatus(string code, string name)
    {
        Code = code;
        Name = name;
    }

    public static TransactionStatus Create(string code)
    {
        return code.ToLower() switch
        {
            "pending" => Pending,
            "completed" => Completed,
            "cancelled" => Cancelled,
            _ => new TransactionStatus(code, code)
        };
    }

    public static TransactionStatus? FromCode(string code)
    {
        return code.ToLower() switch
        {
            "pending" => Pending,
            "completed" => Completed,
            "cancelled" => Cancelled,
            _ => null
        };
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }
}
