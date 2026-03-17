using ErpSystem.Modules.Inventory.Application.Queries.GetProduct;
using ErpSystem.SharedKernel.CQRS;
using ErpSystem.SharedKernel.Pagination;

namespace ErpSystem.Modules.Inventory.Application.Queries.GetProducts;

public sealed record GetProductsQuery(
    string? SearchTerm,
    string? CategoryCode,
    bool? IsActive,
    int PageNumber = 1,
    int PageSize = 10) : IQuery<PagedResult<ProductDto>>;
