using ErpSystem.Infrastructure.Persistence.Repositories;
using ErpSystem.Modules.Orders.Domain.Entities;
using ErpSystem.Modules.Orders.Domain.Repositories;
using ErpSystem.Modules.Orders.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace ErpSystem.Modules.Orders.Infrastructure.Persistence;

public class OrderRepository : Repository<Order, Guid>, IOrderRepository
{
    public OrderRepository(OrdersDbContext context) : base(context)
    {
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);
    }

    public async Task<Order?> GetWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
