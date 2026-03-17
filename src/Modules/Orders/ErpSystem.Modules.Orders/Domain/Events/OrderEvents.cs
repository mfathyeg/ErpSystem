using ErpSystem.SharedKernel.Domain;

namespace ErpSystem.Modules.Orders.Domain.Events;

public sealed record OrderCreatedEvent(Guid OrderId, string OrderNumber, Guid CustomerId) : DomainEvent;

public sealed record OrderSubmittedEvent(Guid OrderId, string OrderNumber, Guid CustomerId, decimal TotalAmount) : DomainEvent;

public sealed record OrderConfirmedEvent(Guid OrderId, string OrderNumber) : DomainEvent;

public sealed record OrderShippedEvent(Guid OrderId, string OrderNumber, string? TrackingNumber) : DomainEvent;

public sealed record OrderDeliveredEvent(Guid OrderId, string OrderNumber) : DomainEvent;

public sealed record OrderCancelledEvent(Guid OrderId, string OrderNumber, string Reason) : DomainEvent;

public sealed record OrderItemAddedEvent(Guid OrderId, Guid ProductId, int Quantity) : DomainEvent;
