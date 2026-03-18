using Asp.Versioning;
using ErpSystem.Domain.Common.ValueObjects;
using ErpSystem.Modules.Inventory.Domain.Entities;
using ErpSystem.Modules.Inventory.Domain.ValueObjects;
using ErpSystem.Modules.Inventory.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpSystem.Modules.Inventory.Api;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly InventoryDbContext _context;

    public InventoryController(InventoryDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetItems(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = "name",
        [FromQuery] string? sortDirection = "asc",
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? category = null)
    {
        var query = _context.Products.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(p =>
                p.Name.Contains(searchTerm) ||
                p.Sku.Contains(searchTerm) ||
                (p.Description != null && p.Description.Contains(searchTerm)));
        }

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Category.Name == category || p.Category.Code == category);
        }

        var totalCount = await query.CountAsync();

        query = sortBy?.ToLower() switch
        {
            "sku" => sortDirection == "asc" ? query.OrderBy(p => p.Sku) : query.OrderByDescending(p => p.Sku),
            "quantity" => sortDirection == "asc" ? query.OrderBy(p => p.StockQuantity.Value) : query.OrderByDescending(p => p.StockQuantity.Value),
            "unitprice" => sortDirection == "asc" ? query.OrderBy(p => p.UnitPrice.Amount) : query.OrderByDescending(p => p.UnitPrice.Amount),
            "category" => sortDirection == "asc" ? query.OrderBy(p => p.Category.Name) : query.OrderByDescending(p => p.Category.Name),
            _ => sortDirection == "asc" ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name)
        };

        var products = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var data = products.Select(p => new InventoryItemDto
        {
            Id = p.Id.ToString(),
            Sku = p.Sku,
            Name = p.Name,
            Description = p.Description ?? string.Empty,
            Category = p.Category.Name,
            Quantity = p.StockQuantity.Value,
            ReorderLevel = p.ReorderLevel,
            UnitPrice = p.UnitPrice.Amount,
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt
        }).ToList();

        return Ok(new PaginatedResponse<InventoryItemDto>
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
    public async Task<IActionResult> GetItem(string id)
    {
        if (!Guid.TryParse(id, out var productId))
            return NotFound(new { message = "المنتج غير موجود" });

        var product = await _context.Products.FindAsync(productId);
        if (product == null)
            return NotFound(new { message = "المنتج غير موجود" });

        return Ok(new InventoryItemDto
        {
            Id = product.Id.ToString(),
            Sku = product.Sku,
            Name = product.Name,
            Description = product.Description ?? string.Empty,
            Category = product.Category.Name,
            Quantity = product.StockQuantity.Value,
            ReorderLevel = product.ReorderLevel,
            UnitPrice = product.UnitPrice.Amount,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt
        });
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateItem([FromBody] CreateInventoryItemRequest request)
    {
        var existingSku = await _context.Products.FirstOrDefaultAsync(p => p.Sku == request.Sku);
        if (existingSku != null)
            return BadRequest(new { message = "رمز المنتج (SKU) موجود بالفعل" });

        var category = ProductCategory.Create(request.Category, request.Category);
        var unitPrice = Money.Create(request.UnitPrice, request.Currency ?? "SAR");

        var product = Product.Create(
            request.Sku,
            request.Name,
            request.Description,
            category,
            unitPrice,
            request.Quantity,
            request.ReorderLevel,
            request.SupplierId);

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetItem), new { id = product.Id }, new InventoryItemDto
        {
            Id = product.Id.ToString(),
            Sku = product.Sku,
            Name = product.Name,
            Description = product.Description ?? string.Empty,
            Category = product.Category.Name,
            Quantity = product.StockQuantity.Value,
            ReorderLevel = product.ReorderLevel,
            UnitPrice = product.UnitPrice.Amount,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt
        });
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateItem(string id, [FromBody] UpdateInventoryItemRequest request)
    {
        if (!Guid.TryParse(id, out var productId))
            return NotFound(new { message = "المنتج غير موجود" });

        var product = await _context.Products.FindAsync(productId);
        if (product == null)
            return NotFound(new { message = "المنتج غير موجود" });

        var category = ProductCategory.Create(request.Category, request.Category);
        var unitPrice = Money.Create(request.UnitPrice, request.Currency ?? "SAR");

        product.UpdateDetails(request.Name, request.Description, category, unitPrice);
        product.SetReorderLevel(request.ReorderLevel);

        await _context.SaveChangesAsync();

        return Ok(new InventoryItemDto
        {
            Id = product.Id.ToString(),
            Sku = product.Sku,
            Name = product.Name,
            Description = product.Description ?? string.Empty,
            Category = product.Category.Name,
            Quantity = product.StockQuantity.Value,
            ReorderLevel = product.ReorderLevel,
            UnitPrice = product.UnitPrice.Amount,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt
        });
    }

    [HttpPatch("{id}/stock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStock(string id, [FromBody] UpdateItemStockRequest request)
    {
        if (!Guid.TryParse(id, out var productId))
            return NotFound(new { message = "المنتج غير موجود" });

        var product = await _context.Products.FindAsync(productId);
        if (product == null)
            return NotFound(new { message = "المنتج غير موجود" });

        try
        {
            var currentQuantity = product.StockQuantity.Value;
            var difference = request.Quantity - currentQuantity;

            if (difference > 0)
            {
                product.AddStock(difference, request.Reason ?? "تعديل يدوي للمخزون");
            }
            else if (difference < 0)
            {
                product.RemoveStock(Math.Abs(difference), request.Reason ?? "تعديل يدوي للمخزون");
            }

            await _context.SaveChangesAsync();

            return Ok(new InventoryItemDto
            {
                Id = product.Id.ToString(),
                Sku = product.Sku,
                Name = product.Name,
                Description = product.Description ?? string.Empty,
                Category = product.Category.Name,
                Quantity = product.StockQuantity.Value,
                ReorderLevel = product.ReorderLevel,
                UnitPrice = product.UnitPrice.Amount,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteItem(string id)
    {
        if (!Guid.TryParse(id, out var productId))
            return NotFound(new { message = "المنتج غير موجود" });

        var product = await _context.Products.FindAsync(productId);
        if (product == null)
            return NotFound(new { message = "المنتج غير موجود" });

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.Products
            .Select(p => p.Category.Name)
            .Distinct()
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("low-stock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLowStockItems()
    {
        var lowStockProducts = await _context.Products
            .Where(p => p.StockQuantity.Value <= p.ReorderLevel)
            .ToListAsync();

        var data = lowStockProducts.Select(p => new InventoryItemDto
        {
            Id = p.Id.ToString(),
            Sku = p.Sku,
            Name = p.Name,
            Description = p.Description ?? string.Empty,
            Category = p.Category.Name,
            Quantity = p.StockQuantity.Value,
            ReorderLevel = p.ReorderLevel,
            UnitPrice = p.UnitPrice.Amount,
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt
        }).ToList();

        return Ok(data);
    }

    [HttpPatch("{id}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateProduct(string id)
    {
        if (!Guid.TryParse(id, out var productId))
            return NotFound(new { message = "المنتج غير موجود" });

        var product = await _context.Products.FindAsync(productId);
        if (product == null)
            return NotFound(new { message = "المنتج غير موجود" });

        product.Activate();
        await _context.SaveChangesAsync();

        return Ok(new { id = product.Id, isActive = product.IsActive });
    }

    [HttpPatch("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateProduct(string id)
    {
        if (!Guid.TryParse(id, out var productId))
            return NotFound(new { message = "المنتج غير موجود" });

        var product = await _context.Products.FindAsync(productId);
        if (product == null)
            return NotFound(new { message = "المنتج غير موجود" });

        product.Deactivate();
        await _context.SaveChangesAsync();

        return Ok(new { id = product.Id, isActive = product.IsActive });
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
    public string Id { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int ReorderLevel { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public record CreateInventoryItemRequest(
    string Sku,
    string Name,
    string? Description,
    string Category,
    int Quantity,
    int ReorderLevel,
    decimal UnitPrice,
    string? Currency = "SAR",
    Guid? SupplierId = null);

public record UpdateInventoryItemRequest(
    string Name,
    string? Description,
    string Category,
    int ReorderLevel,
    decimal UnitPrice,
    string? Currency = "SAR");

public record UpdateItemStockRequest(int Quantity, string? Reason = null);
