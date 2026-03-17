using ErpSystem.SharedKernel.CQRS;

namespace ErpSystem.Modules.Orders.Application.Queries.GetOrder;

public sealed record GetOrderQuery(Guid OrderId) : IQuery<OrderDto>;

public sealed record OrderDto(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    string Status,
    AddressDto ShippingAddress,
    AddressDto? BillingAddress,
    decimal SubTotal,
    decimal Tax,
    decimal ShippingCost,
    decimal Total,
    string Currency,
    string? Notes,
    DateTime? ShippedAt,
    DateTime? DeliveredAt,
    DateTime? CancelledAt,
    string? CancellationReason,
    DateTime CreatedAt,
    List<OrderItemDto> Items);

public sealed record AddressDto(
    string Street,
    string City,
    string State,
    string Country,
    string PostalCode);

public sealed record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string Sku,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);
