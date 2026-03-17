using ErpSystem.Domain.Common.Repositories;
using ErpSystem.Modules.Orders.Domain.Entities;
using ErpSystem.Modules.Orders.Domain.ValueObjects;

namespace ErpSystem.Modules.Orders.Domain.Repositories;

public interface IOrderRepository : IRepository<Order, Guid>
{
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<Order?> GetWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);
}
