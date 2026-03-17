using ErpSystem.Domain.Common.ValueObjects;
using ErpSystem.Modules.Orders.Domain.Events;
using ErpSystem.Modules.Orders.Domain.ValueObjects;
using ErpSystem.SharedKernel.Domain;

namespace ErpSystem.Modules.Orders.Domain.Entities;

public class Order : AuditableAggregateRoot<Guid>
{
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public Address ShippingAddress { get; private set; } = null!;
    public Address? BillingAddress { get; private set; }
    public Money SubTotal { get; private set; } = null!;
    public Money Tax { get; private set; } = null!;
    public Money ShippingCost { get; private set; } = null!;
    public Money Total { get; private set; } = null!;
    public string? Notes { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { }

    public static Order Create(
        Guid customerId,
        Address shippingAddress,
        Address? billingAddress,
        string currency)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = GenerateOrderNumber(),
            CustomerId = customerId,
            Status = OrderStatus.Draft,
            ShippingAddress = shippingAddress,
            BillingAddress = billingAddress,
            SubTotal = Money.Zero(currency),
            Tax = Money.Zero(currency),
            ShippingCost = Money.Zero(currency),
            Total = Money.Zero(currency)
        };

        order.AddDomainEvent(new OrderCreatedEvent(order.Id, order.OrderNumber, customerId));

        return order;
    }

    public void AddItem(Guid productId, string productName, string sku, int quantity, Money unitPrice)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Cannot modify a non-draft order.");

        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem is not null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var item = OrderItem.Create(Id, productId, productName, sku, quantity, unitPrice);
            _items.Add(item);
        }

        RecalculateTotals();
    }

    public void RemoveItem(Guid productId)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Cannot modify a non-draft order.");

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is not null)
        {
            _items.Remove(item);
            RecalculateTotals();
        }
    }

    public void UpdateItemQuantity(Guid productId, int newQuantity)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Cannot modify a non-draft order.");

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
            throw new InvalidOperationException($"Item with product {productId} not found.");

        if (newQuantity <= 0)
        {
            _items.Remove(item);
        }
        else
        {
            item.UpdateQuantity(newQuantity);
        }

        RecalculateTotals();
    }

    public void SetShippingCost(Money shippingCost)
    {
        ShippingCost = shippingCost;
        RecalculateTotals();
    }

    public void SetTaxRate(decimal taxRate)
    {
        Tax = SubTotal.Multiply(taxRate);
        RecalculateTotals();
    }

    public void Submit()
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Only draft orders can be submitted.");

        if (!_items.Any())
            throw new InvalidOperationException("Cannot submit an order with no items.");

        Status = OrderStatus.Pending;
        AddDomainEvent(new OrderSubmittedEvent(Id, OrderNumber, CustomerId, Total.Amount));
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be confirmed.");

        Status = OrderStatus.Confirmed;
        AddDomainEvent(new OrderConfirmedEvent(Id, OrderNumber));
    }

    public void Ship(string? trackingNumber = null)
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed orders can be shipped.");

        Status = OrderStatus.Shipped;
        ShippedAt = DateTime.UtcNow;
        AddDomainEvent(new OrderShippedEvent(Id, OrderNumber, trackingNumber));
    }

    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException("Only shipped orders can be delivered.");

        Status = OrderStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        AddDomainEvent(new OrderDeliveredEvent(Id, OrderNumber));
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Shipped || Status == OrderStatus.Delivered || Status == OrderStatus.Cancelled)
            throw new InvalidOperationException($"Cannot cancel an order with status {Status}.");

        Status = OrderStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;
        AddDomainEvent(new OrderCancelledEvent(Id, OrderNumber, reason));
    }

    private void RecalculateTotals()
    {
        var currency = SubTotal.Currency;
        SubTotal = _items.Aggregate(
            Money.Zero(currency),
            (sum, item) => sum.Add(item.LineTotal));
        Total = SubTotal.Add(Tax).Add(ShippingCost);
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }
}
