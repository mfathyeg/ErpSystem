using Dapper;
using ErpSystem.Application.Abstractions.Data;
using ErpSystem.SharedKernel.CQRS;
using ErpSystem.SharedKernel.Results;

namespace ErpSystem.Modules.Inventory.Application.Queries.GetProduct;

public sealed class GetProductQueryHandler : IQueryHandler<GetProductQuery, ProductDto>
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public GetProductQueryHandler(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<ProductDto>> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        const string sql = """
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
            WHERE p.Id = @ProductId
            """;

        using var connection = _connectionFactory.CreateConnection();

        var product = await connection.QueryFirstOrDefaultAsync<ProductDto>(
            sql,
            new { request.ProductId });

        if (product is null)
        {
            return Result.Failure<ProductDto>(Error.NotFound);
        }

        return Result.Success(product);
    }
}
