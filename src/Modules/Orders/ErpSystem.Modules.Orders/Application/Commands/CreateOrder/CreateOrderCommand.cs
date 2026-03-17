using ErpSystem.SharedKernel.CQRS;

namespace ErpSystem.Modules.Orders.Application.Commands.CreateOrder;

public sealed record CreateOrderCommand(
    Guid CustomerId,
    AddressDto ShippingAddress,
    AddressDto? BillingAddress,
    string Currency,
    List<OrderItemDto> Items) : Command<Guid>;

public sealed record AddressDto(
    string Street,
    string City,
    string State,
    string Country,
    string PostalCode);

public sealed record OrderItemDto(
    Guid ProductId,
    string ProductName,
    string Sku,
    int Quantity,
    decimal UnitPrice);
