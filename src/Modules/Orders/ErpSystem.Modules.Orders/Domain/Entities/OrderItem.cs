using ErpSystem.Domain.Common.ValueObjects;
using ErpSystem.SharedKernel.Domain;

namespace ErpSystem.Modules.Orders.Domain.Entities;

public class OrderItem : Entity<Guid>
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = null!;
    public Money LineTotal { get; private set; } = null!;

    private OrderItem() { }

    internal static OrderItem Create(
        Guid orderId,
        Guid productId,
        string productName,
        string sku,
        int quantity,
        Money unitPrice)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));

        return new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = productId,
            ProductName = productName,
            Sku = sku,
            Quantity = quantity,
            UnitPrice = unitPrice,
            LineTotal = unitPrice.Multiply(quantity)
        };
    }

    internal void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(newQuantity));

        Quantity = newQuantity;
        LineTotal = UnitPrice.Multiply(newQuantity);
    }
}
