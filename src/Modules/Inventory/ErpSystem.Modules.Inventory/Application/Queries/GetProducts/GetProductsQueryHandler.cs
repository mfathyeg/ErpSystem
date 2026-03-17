using System.Text;
using Dapper;
using ErpSystem.Application.Abstractions.Data;
using ErpSystem.Modules.Inventory.Application.Queries.GetProduct;
using ErpSystem.SharedKernel.CQRS;
using ErpSystem.SharedKernel.Pagination;
using ErpSystem.SharedKernel.Results;

namespace ErpSystem.Modules.Inventory.Application.Queries.GetProducts;

public sealed class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public GetProductsQueryHandler(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<PagedResult<ProductDto>>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var whereClause = new StringBuilder("WHERE 1=1");
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            whereClause.Append(" AND (p.Name LIKE @SearchTerm OR p.Sku LIKE @SearchTerm)");
            parameters.Add("SearchTerm", $"%{request.SearchTerm}%");
        }

        if (!string.IsNullOrWhiteSpace(request.CategoryCode))
        {
            whereClause.Append(" AND p.Category_Code = @CategoryCode");
            parameters.Add("CategoryCode", request.CategoryCode);
        }

        if (request.IsActive.HasValue)
        {
            whereClause.Append(" AND p.IsActive = @IsActive");
            parameters.Add("IsActive", request.IsActive.Value);
        }

        var countSql = $"SELECT COUNT(*) FROM Products p {whereClause}";

        var dataSql = $"""
            SELECT
                p.Id,
                p.Sku,
                p.Name,
                p.Description,
                p.Category_Code AS CategoryCode,
                p.Category_Name AS CategoryName,
                p.UnitPrice_Amount AS UnitPrice,
                p.UnitPrice_Currency AS Currency,
                p.StockQuantity_Value AS StockQuantity,
                p.ReorderLevel,
                p.IsActive,
                p.SupplierId,
                p.CreatedAt,
                p.LastModifiedAt
            FROM Products p
            {whereClause}
            ORDER BY p.CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        parameters.Add("Offset", (request.PageNumber - 1) * request.PageSize);
        parameters.Add("PageSize", request.PageSize);

        using var connection = _connectionFactory.CreateConnection();

        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        var products = await connection.QueryAsync<ProductDto>(dataSql, parameters);

        var result = new PagedResult<ProductDto>(
            products.ToList(),
            request.PageNumber,
            request.PageSize,
            totalCount);

        return Result.Success(result);
    }
}
