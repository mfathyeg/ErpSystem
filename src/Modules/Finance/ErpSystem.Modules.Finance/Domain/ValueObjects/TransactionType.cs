using ErpSystem.SharedKernel.Domain;

namespace ErpSystem.Modules.Finance.Domain.ValueObjects;

public sealed class TransactionType : ValueObject
{
    public static TransactionType Income => new("Income", "إيراد");
    public static TransactionType Expense => new("Expense", "مصروف");
    public static TransactionType Transfer => new("Transfer", "تحويل");

    public string Code { get; }
    public string Name { get; }

    private TransactionType(string code, string name)
    {
        Code = code;
        Name = name;
    }

    public static TransactionType Create(string code)
    {
        return code.ToLower() switch
        {
            "income" => Income,
            "expense" => Expense,
            "transfer" => Transfer,
            _ => new TransactionType(code, code)
        };
    }

    public static TransactionType? FromCode(string code)
    {
        return code.ToLower() switch
        {
            "income" => Income,
            "expense" => Expense,
            "transfer" => Transfer,
            _ => null
        };
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }
}
