using ErpSystem.Domain.Common.ValueObjects;
using ErpSystem.Modules.Inventory.Domain.Events;
using ErpSystem.Modules.Inventory.Domain.ValueObjects;
using ErpSystem.SharedKernel.Domain;

namespace ErpSystem.Modules.Inventory.Domain.Entities;

public class Product : AuditableAggregateRoot<Guid>
{
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ProductCategory Category { get; private set; } = null!;
    public Money UnitPrice { get; private set; } = null!;
    public StockQuantity StockQuantity { get; private set; } = null!;
    public int ReorderLevel { get; private set; }
    public bool IsActive { get; private set; }
    public Guid? SupplierId { get; private set; }

    private readonly List<StockMovement> _stockMovements = new();
    public IReadOnlyCollection<StockMovement> StockMovements => _stockMovements.AsReadOnly();

    private Product() { }

    public static Product Create(
        string sku,
        string name,
        string? description,
        ProductCategory category,
        Money unitPrice,
        int initialStock,
        int reorderLevel,
        Guid? supplierId = null)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Sku = sku,
            Name = name,
            Description = description,
            Category = category,
            UnitPrice = unitPrice,
            StockQuantity = StockQuantity.Create(initialStock),
            ReorderLevel = reorderLevel,
            SupplierId = supplierId,
            IsActive = true
        };

        product.AddDomainEvent(new ProductCreatedEvent(product.Id, product.Sku, product.Name));

        return product;
    }

    public void UpdateDetails(string name, string? description, ProductCategory category, Money unitPrice)
    {
        Name = name;
        Description = description;
        Category = category;
        UnitPrice = unitPrice;

        AddDomainEvent(new ProductUpdatedEvent(Id, Sku, Name));
    }

    public void AddStock(int quantity, string reason, Guid? referenceId = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));

        var previousQuantity = StockQuantity.Value;
        StockQuantity = StockQuantity.Add(quantity);

        var movement = StockMovement.Create(
            Id,
            StockMovementType.In,
            quantity,
            previousQuantity,
            StockQuantity.Value,
            reason,
            referenceId);

        _stockMovements.Add(movement);

        AddDomainEvent(new StockAddedEvent(Id, Sku, quantity, StockQuantity.Value));
    }

    public void RemoveStock(int quantity, string reason, Guid? referenceId = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));

        if (!StockQuantity.CanRemove(quantity))
            throw new InvalidOperationException($"Insufficient stock. Available: {StockQuantity.Value}, Requested: {quantity}");

        var previousQuantity = StockQuantity.Value;
        StockQuantity = StockQuantity.Remove(quantity);

        var movement = StockMovement.Create(
            Id,
            StockMovementType.Out,
            quantity,
            previousQuantity,
            StockQuantity.Value,
            reason,
            referenceId);

        _stockMovements.Add(movement);

        AddDomainEvent(new StockRemovedEvent(Id, Sku, quantity, StockQuantity.Value));

        if (StockQuantity.Value <= ReorderLevel)
        {
            AddDomainEvent(new LowStockAlertEvent(Id, Sku, StockQuantity.Value, ReorderLevel));
        }
    }

    public void Deactivate()
    {
        IsActive = false;
        AddDomainEvent(new ProductDeactivatedEvent(Id, Sku));
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void SetReorderLevel(int reorderLevel)
    {
        if (reorderLevel < 0)
            throw new ArgumentException("Reorder level cannot be negative.", nameof(reorderLevel));

        ReorderLevel = reorderLevel;
    }
}
