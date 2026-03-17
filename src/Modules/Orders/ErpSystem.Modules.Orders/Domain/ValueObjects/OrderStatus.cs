using ErpSystem.SharedKernel.Domain;

namespace ErpSystem.Modules.Orders.Domain.ValueObjects;

public sealed class OrderStatus : Enumeration<OrderStatus>
{
    public static readonly OrderStatus Draft = new(1, nameof(Draft));
    public static readonly OrderStatus Pending = new(2, nameof(Pending));
    public static readonly OrderStatus Confirmed = new(3, nameof(Confirmed));
    public static readonly OrderStatus Processing = new(4, nameof(Processing));
    public static readonly OrderStatus Shipped = new(5, nameof(Shipped));
    public static readonly OrderStatus Delivered = new(6, nameof(Delivered));
    public static readonly OrderStatus Cancelled = new(7, nameof(Cancelled));
    public static readonly OrderStatus Refunded = new(8, nameof(Refunded));

    private OrderStatus(int id, string name) : base(id, name)
    {
    }

    public bool CanTransitionTo(OrderStatus newStatus)
    {
        return (this, newStatus) switch
        {
            (var current, var next) when current == Draft && next == Pending => true,
            (var current, var next) when current == Pending && next == Confirmed => true,
            (var current, var next) when current == Confirmed && next == Processing => true,
            (var current, var next) when current == Processing && next == Shipped => true,
            (var current, var next) when current == Confirmed && next == Shipped => true,
            (var current, var next) when current == Shipped && next == Delivered => true,
            (var current, var next) when current == Draft && next == Cancelled => true,
            (var current, var next) when current == Pending && next == Cancelled => true,
            (var current, var next) when current == Confirmed && next == Cancelled => true,
            (var current, var next) when current == Delivered && next == Refunded => true,
            _ => false
        };
    }
}
