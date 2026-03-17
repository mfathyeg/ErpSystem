using ErpSystem.SharedKernel.Domain;

namespace ErpSystem.Modules.Orders.IntegrationEvents;

public sealed record OrderConfirmedIntegrationEvent(
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId,
    List<OrderItemInfo> Items,
    decimal TotalAmount,
    string Currency) : IntegrationEvent;

public sealed record OrderItemInfo(
    Guid ProductId,
    string Sku,
    int Quantity);
