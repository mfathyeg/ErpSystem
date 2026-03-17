using ErpSystem.SharedKernel.CQRS;

namespace ErpSystem.Modules.Inventory.Application.Queries.GetProduct;

public sealed record GetProductQuery(Guid ProductId) : IQuery<ProductDto>;

public sealed record ProductDto(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    string CategoryCode,
    string CategoryName,
    decimal UnitPrice,
    string Currency,
    int StockQuantity,
    int ReorderLevel,
    bool IsActive,
    Guid? SupplierId,
    DateTime CreatedAt,
    DateTime? LastModifiedAt);
