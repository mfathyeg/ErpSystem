using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ErpSystem.Modules.Orders.Api;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private static readonly List<OrderDto> _orders = GenerateMockOrders();

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetOrders(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = "orderDate",
        [FromQuery] string? sortDirection = "desc",
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? status = null)
    {
        var query = _orders.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(o =>
                o.OrderNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                o.CustomerName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(o => o.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        query = sortBy?.ToLower() switch
        {
            "ordernumber" => sortDirection == "asc" ? query.OrderBy(o => o.OrderNumber) : query.OrderByDescending(o => o.OrderNumber),
            "customername" => sortDirection == "asc" ? query.OrderBy(o => o.CustomerName) : query.OrderByDescending(o => o.CustomerName),
            "totalamount" => sortDirection == "asc" ? query.OrderBy(o => o.TotalAmount) : query.OrderByDescending(o => o.TotalAmount),
            "status" => sortDirection == "asc" ? query.OrderBy(o => o.Status) : query.OrderByDescending(o => o.Status),
            _ => sortDirection == "asc" ? query.OrderBy(o => o.OrderDate) : query.OrderByDescending(o => o.OrderDate)
        };

        var totalCount = query.Count();
        var data = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return Ok(new PaginatedResponse<OrderDto>
        {
            Data = data,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetOrder(int id)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
            return NotFound(new { message = "الطلب غير موجود" });

        return Ok(order);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public IActionResult CreateOrder([FromBody] CreateOrderRequest request)
    {
        var newId = _orders.Max(o => o.Id) + 1;
        var order = new OrderDto
        {
            Id = newId,
            OrderNumber = $"ORD-2024-{newId:D3}",
            CustomerId = 1,
            CustomerName = "عميل جديد",
            Status = "Pending",
            OrderDate = DateTime.UtcNow,
            TotalAmount = request.Items.Sum(i => i.Quantity * i.UnitPrice),
            ShippingAddress = new AddressDto
            {
                Street = request.ShippingAddress.Street,
                City = request.ShippingAddress.City,
                State = request.ShippingAddress.State,
                PostalCode = request.ShippingAddress.PostalCode,
                Country = request.ShippingAddress.Country
            },
            BillingAddress = request.BillingAddress != null ? new AddressDto
            {
                Street = request.BillingAddress.Street,
                City = request.BillingAddress.City,
                State = request.BillingAddress.State,
                PostalCode = request.BillingAddress.PostalCode,
                Country = request.BillingAddress.Country
            } : null,
            Items = request.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Sku = i.Sku,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList(),
            CreatedAt = DateTime.UtcNow
        };

        _orders.Add(order);
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
            return NotFound(new { message = "الطلب غير موجود" });

        order.Status = request.Status;
        return Ok(order);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteOrder(int id)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
            return NotFound(new { message = "الطلب غير موجود" });

        _orders.Remove(order);
        return NoContent();
    }

    private static List<OrderDto> GenerateMockOrders()
    {
        var statuses = new[] { "Pending", "Confirmed", "Processing", "Shipped", "Delivered", "Cancelled" };
        var customers = new[]
        {
            ("أحمد محمد", "الرياض"),
            ("سارة علي", "جدة"),
            ("خالد عبدالله", "الدمام"),
            ("نورة سعيد", "مكة"),
            ("فهد العتيبي", "المدينة"),
            ("ريم الشمري", "الخبر"),
            ("محمد القحطاني", "تبوك"),
            ("لينا الحربي", "أبها")
        };

        var orders = new List<OrderDto>();
        var random = new Random(42);

        for (int i = 1; i <= 50; i++)
        {
            var customer = customers[random.Next(customers.Length)];
            orders.Add(new OrderDto
            {
                Id = i,
                OrderNumber = $"ORD-2024-{i:D3}",
                CustomerId = i,
                CustomerName = customer.Item1,
                Status = statuses[random.Next(statuses.Length)],
                OrderDate = DateTime.UtcNow.AddDays(-random.Next(1, 60)),
                TotalAmount = Math.Round((decimal)(random.NextDouble() * 5000 + 100), 2),
                ShippingAddress = new AddressDto
                {
                    Street = $"شارع {random.Next(1, 100)}",
                    City = customer.Item2,
                    State = customer.Item2,
                    PostalCode = $"{random.Next(10000, 99999)}",
                    Country = "السعودية"
                },
                Items = new List<OrderItemDto>(),
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 60))
            });
        }

        return orders;
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
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
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
    string Currency,
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

public record UpdateStatusRequest(string Status);
