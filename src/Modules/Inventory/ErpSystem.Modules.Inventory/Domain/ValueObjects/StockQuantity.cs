using ErpSystem.SharedKernel.Domain;

namespace ErpSystem.Modules.Inventory.Domain.ValueObjects;

public sealed class StockQuantity : ValueObject
{
    public int Value { get; }

    private StockQuantity(int value)
    {
        Value = value;
    }

    public static StockQuantity Create(int value)
    {
        if (value < 0)
            throw new ArgumentException("Stock quantity cannot be negative.", nameof(value));

        return new StockQuantity(value);
    }

    public static StockQuantity Zero => new(0);

    public bool CanRemove(int quantity) => Value >= quantity;

    public StockQuantity Add(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Quantity to add cannot be negative.", nameof(quantity));

        return new StockQuantity(Value + quantity);
    }

    public StockQuantity Remove(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Quantity to remove cannot be negative.", nameof(quantity));

        if (!CanRemove(quantity))
            throw new InvalidOperationException("Cannot remove more than available stock.");

        return new StockQuantity(Value - quantity);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator int(StockQuantity quantity) => quantity.Value;
}
