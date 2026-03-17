using Dapper;
using ErpSystem.Application.Abstractions.Data;
using ErpSystem.SharedKernel.CQRS;
using ErpSystem.SharedKernel.Results;

namespace ErpSystem.Modules.Orders.Application.Queries.GetOrder;

public sealed class GetOrderQueryHandler : IQueryHandler<GetOrderQuery, OrderDto>
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public GetOrderQueryHandler(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<OrderDto>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        const string orderSql = """
            SELECT
                o.Id,
                o.OrderNumber,
                o.CustomerId,
                o.Status_Name AS Status,
                o.ShippingAddress_Street AS ShippingStreet,
                o.ShippingAddress_City AS ShippingCity,
                o.ShippingAddress_State AS ShippingState,
                o.ShippingAddress_Country AS ShippingCountry,
                o.ShippingAddress_PostalCode AS ShippingPostalCode,
                o.BillingAddress_Street AS BillingStreet,
                o.BillingAddress_City AS BillingCity,
                o.BillingAddress_State AS BillingState,
                o.BillingAddress_Country AS BillingCountry,
                o.BillingAddress_PostalCode AS BillingPostalCode,
                o.SubTotal_Amount AS SubTotal,
                o.Tax_Amount AS Tax,
                o.ShippingCost_Amount AS ShippingCost,
                o.Total_Amount AS Total,
                o.SubTotal_Currency AS Currency,
                o.Notes,
                o.ShippedAt,
                o.DeliveredAt,
                o.CancelledAt,
                o.CancellationReason,
                o.CreatedAt
            FROM [Orders].Orders o
            WHERE o.Id = @OrderId
            """;

        const string itemsSql = """
            SELECT
                oi.Id,
                oi.ProductId,
                oi.ProductName,
                oi.Sku,
                oi.Quantity,
                oi.UnitPrice_Amount AS UnitPrice,
                oi.LineTotal_Amount AS LineTotal
            FROM [Orders].OrderItems oi
            WHERE oi.OrderId = @OrderId
            """;

        using var connection = _connectionFactory.CreateConnection();

        var orderData = await connection.QueryFirstOrDefaultAsync<dynamic>(orderSql, new { request.OrderId });

        if (orderData is null)
        {
            return Result.Failure<OrderDto>(Error.NotFound);
        }

        var items = await connection.QueryAsync<OrderItemDto>(itemsSql, new { request.OrderId });

        var shippingAddress = new AddressDto(
            orderData.ShippingStreet,
            orderData.ShippingCity,
            orderData.ShippingState,
            orderData.ShippingCountry,
            orderData.ShippingPostalCode);

        AddressDto? billingAddress = null;
        if (orderData.BillingStreet is not null)
        {
            billingAddress = new AddressDto(
                orderData.BillingStreet,
                orderData.BillingCity,
                orderData.BillingState,
                orderData.BillingCountry,
                orderData.BillingPostalCode);
        }

        var order = new OrderDto(
            orderData.Id,
            orderData.OrderNumber,
            orderData.CustomerId,
            orderData.Status,
            shippingAddress,
            billingAddress,
            orderData.SubTotal,
            orderData.Tax,
            orderData.ShippingCost,
            orderData.Total,
            orderData.Currency,
            orderData.Notes,
            orderData.ShippedAt,
            orderData.DeliveredAt,
            orderData.CancelledAt,
            orderData.CancellationReason,
            orderData.CreatedAt,
            items.ToList());

        return Result.Success(order);
    }
}
