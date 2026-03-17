using ErpSystem.Domain.Common.Repositories;
using ErpSystem.Modules.Inventory.Domain.Entities;

namespace ErpSystem.Modules.Inventory.Domain.Repositories;

public interface IProductRepository : IRepository<Product, Guid>
{
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetByCategoryAsync(string categoryCode, CancellationToken cancellationToken = default);
    Task<Product?> GetWithMovementsAsync(Guid productId, CancellationToken cancellationToken = default);
}
