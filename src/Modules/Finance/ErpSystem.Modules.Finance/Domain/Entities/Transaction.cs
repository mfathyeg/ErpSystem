using ErpSystem.Domain.Common.ValueObjects;
using ErpSystem.Modules.Finance.Domain.ValueObjects;
using ErpSystem.SharedKernel.Domain;

namespace ErpSystem.Modules.Finance.Domain.Entities;

public class Transaction : AuditableAggregateRoot<Guid>
{
    public string Reference { get; private set; } = string.Empty;
    public TransactionType Type { get; private set; } = null!;
    public string Category { get; private set; } = string.Empty;
    public Money Amount { get; private set; } = null!;
    public string Description { get; private set; } = string.Empty;
    public TransactionStatus Status { get; private set; } = null!;
    public DateTime TransactionDate { get; private set; }
    public Guid? RelatedEntityId { get; private set; }
    public string? RelatedEntityType { get; private set; }

    private Transaction() { }

    public static Transaction Create(
        TransactionType type,
        string category,
        Money amount,
        string description,
        DateTime? transactionDate = null,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null)
    {
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            Reference = GenerateReference(),
            Type = type,
            Category = category,
            Amount = amount,
            Description = description,
            Status = TransactionStatus.Pending,
            TransactionDate = transactionDate ?? DateTime.UtcNow,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType
        };

        return transaction;
    }

    public void Complete()
    {
        if (Status.Code == TransactionStatus.Cancelled.Code)
            throw new InvalidOperationException("لا يمكن إكمال معاملة ملغاة");

        Status = TransactionStatus.Completed;
    }

    public void Cancel(string reason)
    {
        if (Status.Code == TransactionStatus.Completed.Code)
            throw new InvalidOperationException("لا يمكن إلغاء معاملة مكتملة");

        Status = TransactionStatus.Cancelled;
        Description = $"{Description} - سبب الإلغاء: {reason}";
    }

    public void UpdateDetails(string category, string description)
    {
        if (Status.Code != TransactionStatus.Pending.Code)
            throw new InvalidOperationException("لا يمكن تعديل معاملة غير معلقة");

        Category = category;
        Description = description;
    }

    private static string GenerateReference()
    {
        return $"TXN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }
}
