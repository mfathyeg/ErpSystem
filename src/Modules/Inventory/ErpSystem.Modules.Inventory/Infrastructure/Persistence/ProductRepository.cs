using ErpSystem.Infrastructure.Persistence.Repositories;
using ErpSystem.Modules.Inventory.Domain.Entities;
using ErpSystem.Modules.Inventory.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ErpSystem.Modules.Inventory.Infrastructure.Persistence;

public class ProductRepository : Repository<Product, Guid>, IProductRepository
{
    public ProductRepository(InventoryDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(p => p.Sku == sku, cancellationToken);
    }

    public async Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(p => p.Sku == sku, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(p => p.IsActive && p.StockQuantity.Value <= p.ReorderLevel)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(
        string categoryCode,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(p => p.Category.Code == categoryCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetWithMovementsAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.StockMovements)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
    }
}
