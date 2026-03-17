using ErpSystem.SharedKernel.Domain;

namespace ErpSystem.Modules.Inventory.Domain.Entities;

public class StockMovement : Entity<Guid>
{
    public Guid ProductId { get; private set; }
    public StockMovementType MovementType { get; private set; }
    public int Quantity { get; private set; }
    public int PreviousQuantity { get; private set; }
    public int NewQuantity { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public Guid? ReferenceId { get; private set; }
    public DateTime OccurredAt { get; private set; }

    private StockMovement() { }

    public static StockMovement Create(
        Guid productId,
        StockMovementType movementType,
        int quantity,
        int previousQuantity,
        int newQuantity,
        string reason,
        Guid? referenceId = null)
    {
        return new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            MovementType = movementType,
            Quantity = quantity,
            PreviousQuantity = previousQuantity,
            NewQuantity = newQuantity,
            Reason = reason,
            ReferenceId = referenceId,
            OccurredAt = DateTime.UtcNow
        };
    }
}

public enum StockMovementType
{
    In = 1,
    Out = 2,
    Adjustment = 3,
    Transfer = 4
}
