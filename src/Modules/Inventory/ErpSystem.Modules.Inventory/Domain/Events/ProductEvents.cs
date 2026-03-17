using ErpSystem.SharedKernel.Domain;

namespace ErpSystem.Modules.Inventory.Domain.Events;

public sealed record ProductCreatedEvent(Guid ProductId, string Sku, string Name) : DomainEvent;

public sealed record ProductUpdatedEvent(Guid ProductId, string Sku, string Name) : DomainEvent;

public sealed record ProductDeactivatedEvent(Guid ProductId, string Sku) : DomainEvent;

public sealed record StockAddedEvent(Guid ProductId, string Sku, int QuantityAdded, int NewQuantity) : DomainEvent;

public sealed record StockRemovedEvent(Guid ProductId, string Sku, int QuantityRemoved, int NewQuantity) : DomainEvent;

public sealed record LowStockAlertEvent(Guid ProductId, string Sku, int CurrentQuantity, int ReorderLevel) : DomainEvent;
