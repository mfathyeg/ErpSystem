using ErpSystem.SharedKernel.CQRS;

namespace ErpSystem.Modules.Inventory.Application.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string Sku,
    string Name,
    string? Description,
    string CategoryCode,
    string CategoryName,
    decimal UnitPrice,
    string Currency,
    int InitialStock,
    int ReorderLevel,
    Guid? SupplierId) : Command<Guid>;
