using Asp.Versioning;
using ErpSystem.Domain.Common.ValueObjects;
using ErpSystem.Modules.Orders.Domain.Entities;
using ErpSystem.Modules.Orders.Domain.ValueObjects;
using ErpSystem.Modules.Orders.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpSystem.Modules.Orders.Api;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrdersDbContext _context;

    public OrdersController(OrdersDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = "orderDate",
        [FromQuery] string? sortDirection = "desc",
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? status = null)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(o => o.OrderNumber.Contains(searchTerm));
        }

        if (!string.IsNullOrEmpty(status))
        {
            var orderStatus = OrderStatus.FromName(status);
            if (orderStatus != null)
            {
                query = query.Where(o => o.Status == orderStatus);
            }
        }

        var totalCount = await query.CountAsync();

        query = sortBy?.ToLower() switch
        {
            "ordernumber" => sortDirection == "asc" ? query.OrderBy(o => o.OrderNumber) : query.OrderByDescending(o => o.OrderNumber),
            "totalamount" => sortDirection == "asc" ? query.OrderBy(o => o.Total.Amount) : query.OrderByDescending(o => o.Total.Amount),
            _ => sortDirection == "asc" ? query.OrderBy(o => o.CreatedAt) : query.OrderByDescending(o => o.CreatedAt)
        };

        var orders = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var data = orders.Select(o => new OrderDto
        {
            Id = o.Id.ToString(),
            OrderNumber = o.OrderNumber,
            CustomerId = o.CustomerId.ToString(),
            CustomerName = "عميل", // Would need to join with customer table
            Status = o.Status.Name,
            OrderDate = o.CreatedAt,
            TotalAmount = o.Total.Amount,
            ShippingAddress = new AddressDto
            {
                Street = o.ShippingAddress.Street,
                City = o.ShippingAddress.City,
                State = o.ShippingAddress.State,
                PostalCode = o.ShippingAddress.PostalCode,
                Country = o.ShippingAddress.Country
            },
            BillingAddress = o.BillingAddress != null ? new AddressDto
            {
                Street = o.BillingAddress.Street,
                City = o.BillingAddress.City,
                State = o.BillingAddress.State,
                PostalCode = o.BillingAddress.PostalCode,
                Country = o.BillingAddress.Country
            } : null,
            Items = o.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Sku = i.Sku,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice.Amount
            }).ToList(),
            CreatedAt = o.CreatedAt
        }).ToList();

        return Ok(new PaginatedResponse<OrderDto>
        {
            Data = data,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(string id)
    {
        if (!Guid.TryParse(id, out var orderId))
            return NotFound(new { message = "الطلب غير موجود" });

        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            return NotFound(new { message = "الطلب غير موجود" });

        return Ok(new OrderDto
        {
            Id = order.Id.ToString(),
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId.ToString(),
            CustomerName = "عميل",
            Status = order.Status.Name,
            OrderDate = order.CreatedAt,
            TotalAmount = order.Total.Amount,
            ShippingAddress = new AddressDto
            {
                Street = order.ShippingAddress.Street,
                City = order.ShippingAddress.City,
                State = order.ShippingAddress.State,
                PostalCode = order.ShippingAddress.PostalCode,
                Country = order.ShippingAddress.Country
            },
            BillingAddress = order.BillingAddress != null ? new AddressDto
            {
                Street = order.BillingAddress.Street,
                City = order.BillingAddress.City,
                State = order.BillingAddress.State,
                PostalCode = order.BillingAddress.PostalCode,
                Country = order.BillingAddress.Country
            } : null,
            Items = order.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Sku = i.Sku,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice.Amount
            }).ToList(),
            CreatedAt = order.CreatedAt
        });
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var shippingAddress = Address.Create(
            request.ShippingAddress.Street,
            request.ShippingAddress.City,
            request.ShippingAddress.State,
            request.ShippingAddress.Country,
            request.ShippingAddress.PostalCode);

        Address? billingAddress = request.BillingAddress != null
            ? Address.Create(
                request.BillingAddress.Street,
                request.BillingAddress.City,
                request.BillingAddress.State,
                request.BillingAddress.Country,
                request.BillingAddress.PostalCode)
            : null;

        var order = Order.Create(
            request.CustomerId,
            shippingAddress,
            billingAddress,
            request.Currency ?? "SAR");

        foreach (var item in request.Items)
        {
            order.AddItem(
                item.ProductId,
                item.ProductName,
                item.Sku,
                item.Quantity,
                Money.Create(item.UnitPrice, request.Currency ?? "SAR"));
        }

        order.Submit();

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, new { id = order.Id, orderNumber = order.OrderNumber });
    }

    [HttpPatch("{id}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateStatusRequest request)
    {
        if (!Guid.TryParse(id, out var orderId))
            return NotFound(new { message = "الطلب غير موجود" });

        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
            return NotFound(new { message = "الطلب غير موجود" });

        try
        {
            switch (request.Status.ToLower())
            {
                case "confirmed":
                    order.Confirm();
                    break;
                case "shipped":
                    order.Ship();
                    break;
                case "delivered":
                    order.Deliver();
                    break;
                case "cancelled":
                    order.Cancel(request.Reason ?? "تم الإلغاء بواسطة المستخدم");
                    break;
                default:
                    return BadRequest(new { message = "حالة غير صالحة" });
            }

            await _context.SaveChangesAsync();
            return Ok(new { id = order.Id, status = order.Status.Name });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOrder(string id)
    {
        if (!Guid.TryParse(id, out var orderId))
            return NotFound(new { message = "الطلب غير موجود" });

        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
            return NotFound(new { message = "الطلب غير موجود" });

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class PaginatedResponse<T>
{
    public List<T> Data { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class OrderDto
{
    public string Id { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public AddressDto? ShippingAddress { get; set; }
    public AddressDto? BillingAddress { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class AddressDto
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public class OrderItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public record CreateOrderRequest(
    Guid CustomerId,
    AddressRequest ShippingAddress,
    AddressRequest? BillingAddress,
    string? Currency,
    List<OrderItemRequest> Items);

public record AddressRequest(
    string Street,
    string City,
    string State,
    string Country,
    string PostalCode);

public record OrderItemRequest(
    Guid ProductId,
    string ProductName,
    string Sku,
    int Quantity,
    decimal UnitPrice);

public record UpdateStatusRequest(string Status, string? Reason = null);
