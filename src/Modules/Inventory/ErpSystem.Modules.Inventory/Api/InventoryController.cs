using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ErpSystem.Modules.Inventory.Api;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private static readonly List<InventoryItemDto> _items = GenerateMockItems();

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetItems(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = "name",
        [FromQuery] string? sortDirection = "asc",
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? category = null)
    {
        var query = _items.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(i =>
                i.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                i.Sku.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                i.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(i => i.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        query = sortBy?.ToLower() switch
        {
            "sku" => sortDirection == "asc" ? query.OrderBy(i => i.Sku) : query.OrderByDescending(i => i.Sku),
            "quantity" => sortDirection == "asc" ? query.OrderBy(i => i.Quantity) : query.OrderByDescending(i => i.Quantity),
            "unitprice" => sortDirection == "asc" ? query.OrderBy(i => i.UnitPrice) : query.OrderByDescending(i => i.UnitPrice),
            "category" => sortDirection == "asc" ? query.OrderBy(i => i.Category) : query.OrderByDescending(i => i.Category),
            _ => sortDirection == "asc" ? query.OrderBy(i => i.Name) : query.OrderByDescending(i => i.Name)
        };

        var totalCount = query.Count();
        var data = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return Ok(new PaginatedResponse<InventoryItemDto>
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
    public IActionResult GetItem(int id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item == null)
            return NotFound(new { message = "المنتج غير موجود" });

        return Ok(item);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public IActionResult CreateItem([FromBody] CreateInventoryItemRequest request)
    {
        var newId = _items.Max(i => i.Id) + 1;
        var item = new InventoryItemDto
        {
            Id = newId,
            Sku = request.Sku,
            Name = request.Name,
            Description = request.Description ?? string.Empty,
            Category = request.Category,
            Quantity = request.Quantity,
            ReorderLevel = request.ReorderLevel,
            UnitPrice = request.UnitPrice,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _items.Add(item);
        return CreatedAtAction(nameof(GetItem), new { id = item.Id }, item);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UpdateItem(int id, [FromBody] UpdateInventoryItemRequest request)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item == null)
            return NotFound(new { message = "المنتج غير موجود" });

        item.Name = request.Name;
        item.Description = request.Description ?? string.Empty;
        item.Category = request.Category;
        item.UnitPrice = request.UnitPrice;
        item.ReorderLevel = request.ReorderLevel;
        item.UpdatedAt = DateTime.UtcNow;

        return Ok(item);
    }

    [HttpPatch("{id:int}/stock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UpdateStock(int id, [FromBody] UpdateItemStockRequest request)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item == null)
            return NotFound(new { message = "المنتج غير موجود" });

        item.Quantity = request.Quantity;
        item.UpdatedAt = DateTime.UtcNow;

        return Ok(item);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteItem(int id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item == null)
            return NotFound(new { message = "المنتج غير موجود" });

        _items.Remove(item);
        return NoContent();
    }

    [HttpGet("categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetCategories()
    {
        var categories = _items.Select(i => i.Category).Distinct().ToList();
        return Ok(categories);
    }

    [HttpGet("low-stock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetLowStockItems()
    {
        var lowStockItems = _items.Where(i => i.Quantity <= i.ReorderLevel).ToList();
        return Ok(lowStockItems);
    }

    private static List<InventoryItemDto> GenerateMockItems()
    {
        var categories = new[] { "Electronics", "Furniture", "Office", "Accessories", "Storage" };
        var products = new[]
        {
            ("شاشة كمبيوتر 27 بوصة", "Electronics", 1500.00m),
            ("كرسي مكتب مريح", "Furniture", 850.00m),
            ("لوحة مفاتيح لاسلكية", "Accessories", 250.00m),
            ("ماوس احترافي", "Accessories", 180.00m),
            ("مكتب خشبي", "Furniture", 2200.00m),
            ("طابعة ليزر", "Electronics", 1200.00m),
            ("سماعات رأس", "Accessories", 450.00m),
            ("كاميرا ويب HD", "Electronics", 320.00m),
            ("حامل شاشة", "Accessories", 280.00m),
            ("خزانة ملفات", "Storage", 650.00m),
            ("لابتوب Dell", "Electronics", 4500.00m),
            ("كرسي زوار", "Furniture", 350.00m),
            ("قلم ذكي", "Accessories", 120.00m),
            ("شاحن لاسلكي", "Accessories", 95.00m),
            ("رف كتب", "Storage", 420.00m)
        };

        var items = new List<InventoryItemDto>();
        var random = new Random(42);

        for (int i = 0; i < products.Length; i++)
        {
            var product = products[i];
            items.Add(new InventoryItemDto
            {
                Id = i + 1,
                Sku = $"SKU-{(i + 1):D4}",
                Name = product.Item1,
                Description = $"وصف {product.Item1}",
                Category = product.Item2,
                Quantity = random.Next(0, 150),
                ReorderLevel = random.Next(10, 30),
                UnitPrice = product.Item3,
                IsActive = random.Next(10) > 1,
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 180))
            });
        }

        return items;
    }
}

public class PaginatedResponse<T>
{
    public List<T> Data { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class InventoryItemDto
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int ReorderLevel { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public record CreateInventoryItemRequest(
    string Sku,
    string Name,
    string? Description,
    string Category,
    int Quantity,
    int ReorderLevel,
    decimal UnitPrice);

public record UpdateInventoryItemRequest(
    string Name,
    string? Description,
    string Category,
    int ReorderLevel,
    decimal UnitPrice);

public record UpdateItemStockRequest(int Quantity);
