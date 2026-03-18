using Asp.Versioning;
using ErpSystem.Modules.Finance.Domain.ValueObjects;
using ErpSystem.Modules.Finance.Infrastructure.Persistence;
using ErpSystem.Modules.Identity.Models;
using ErpSystem.Modules.Inventory.Infrastructure.Persistence;
using ErpSystem.Modules.Orders.Domain.ValueObjects;
using ErpSystem.Modules.Orders.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpSystem.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly OrdersDbContext _ordersContext;
    private readonly InventoryDbContext _inventoryContext;
    private readonly FinanceDbContext _financeContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(
        OrdersDbContext ordersContext,
        InventoryDbContext inventoryContext,
        FinanceDbContext financeContext,
        UserManager<ApplicationUser> userManager)
    {
        _ordersContext = ordersContext;
        _inventoryContext = inventoryContext;
        _financeContext = financeContext;
        _userManager = userManager;
    }

    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats()
    {
        var totalOrders = await _ordersContext.Orders.CountAsync();
        var totalProducts = await _inventoryContext.Products.CountAsync();
        var totalCustomers = await _userManager.Users.CountAsync();

        var transactions = await _financeContext.Transactions.ToListAsync();
        var totalRevenue = transactions
            .Where(t => t.Type.Code == TransactionType.Income.Code && t.Status.Code == TransactionStatus.Completed.Code)
            .Sum(t => t.Amount.Amount);

        var lastMonthStart = DateTime.UtcNow.AddDays(-30);
        var previousMonthStart = DateTime.UtcNow.AddDays(-60);

        var currentMonthOrders = await _ordersContext.Orders.CountAsync(o => o.CreatedAt >= lastMonthStart);
        var previousMonthOrders = await _ordersContext.Orders.CountAsync(o => o.CreatedAt >= previousMonthStart && o.CreatedAt < lastMonthStart);
        var ordersGrowth = previousMonthOrders > 0
            ? ((decimal)(currentMonthOrders - previousMonthOrders) / previousMonthOrders) * 100
            : 0;

        return Ok(new
        {
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            TotalProducts = totalProducts,
            TotalCustomers = totalCustomers,
            RevenueGrowth = 0m,
            OrdersGrowth = Math.Round(ordersGrowth, 1),
            ProductsGrowth = 0m,
            CustomersGrowth = 0m
        });
    }

    [HttpGet("recent-orders")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecentOrders()
    {
        var orders = await _ordersContext.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Take(5)
            .ToListAsync();

        var data = orders.Select(o => new
        {
            Id = o.Id.ToString(),
            OrderNumber = o.OrderNumber,
            CustomerName = "عميل",
            Status = o.Status.Name,
            TotalAmount = o.Total.Amount,
            OrderDate = o.CreatedAt
        });

        return Ok(data);
    }

    [HttpGet("top-products")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopProducts()
    {
        var products = await _inventoryContext.Products
            .OrderByDescending(p => p.StockQuantity.Value)
            .Take(5)
            .ToListAsync();

        var data = products.Select(p => new
        {
            Id = p.Id.ToString(),
            Name = p.Name,
            Category = p.Category.Name,
            Stock = p.StockQuantity.Value,
            Price = p.UnitPrice.Amount
        });

        return Ok(data);
    }

    [HttpGet("revenue-chart")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRevenueChart()
    {
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);

        var transactions = await _financeContext.Transactions
            .Where(t => t.TransactionDate >= sixMonthsAgo)
            .ToListAsync();

        var orders = await _ordersContext.Orders
            .Where(o => o.CreatedAt >= sixMonthsAgo)
            .ToListAsync();

        var monthlyData = Enumerable.Range(0, 6)
            .Select(i =>
            {
                var monthStart = DateTime.UtcNow.AddMonths(-5 + i);
                var monthEnd = monthStart.AddMonths(1);
                var monthName = GetArabicMonthName(monthStart.Month);

                var revenue = transactions
                    .Where(t => t.TransactionDate >= monthStart && t.TransactionDate < monthEnd
                                && t.Type.Code == TransactionType.Income.Code)
                    .Sum(t => t.Amount.Amount);

                var orderCount = orders
                    .Count(o => o.CreatedAt >= monthStart && o.CreatedAt < monthEnd);

                return new { Month = monthName, Revenue = revenue, Orders = orderCount };
            })
            .ToList();

        return Ok(monthlyData);
    }

    [HttpGet("team-members")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTeamMembers()
    {
        var users = await _userManager.Users
            .Where(u => u.IsActive)
            .Take(5)
            .ToListAsync();

        var members = new List<object>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            members.Add(new
            {
                Id = user.Id.ToString(),
                Name = $"{user.FirstName} {user.LastName}",
                Role = roles.FirstOrDefault() ?? "موظف",
                Avatar = user.FirstName.FirstOrDefault().ToString().ToUpper(),
                Status = user.LastLoginAt.HasValue && user.LastLoginAt.Value > DateTime.UtcNow.AddMinutes(-30) ? "online" : "offline"
            });
        }

        return Ok(members);
    }

    [HttpGet("activity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecentActivity()
    {
        var recentOrders = await _ordersContext.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Take(5)
            .ToListAsync();

        var activities = recentOrders.Select((o, i) => new
        {
            Id = i + 1,
            User = "مستخدم",
            Action = o.Status.Name == OrderStatus.Pending.Name ? "أنشأ طلب جديد" :
                     o.Status.Name == OrderStatus.Confirmed.Name ? "أكد الطلب" :
                     o.Status.Name == OrderStatus.Shipped.Name ? "شحن الطلب" :
                     o.Status.Name == OrderStatus.Delivered.Name ? "سلّم الطلب" : "حدّث الطلب",
            Target = $"#{o.OrderNumber}",
            Time = o.CreatedAt
        }).ToList();

        return Ok(activities);
    }

    [HttpGet("low-stock-alerts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLowStockAlerts()
    {
        var lowStockProducts = await _inventoryContext.Products
            .Where(p => p.StockQuantity.Value <= p.ReorderLevel && p.IsActive)
            .Take(5)
            .ToListAsync();

        var data = lowStockProducts.Select(p => new
        {
            Id = p.Id.ToString(),
            Name = p.Name,
            Sku = p.Sku,
            CurrentStock = p.StockQuantity.Value,
            ReorderLevel = p.ReorderLevel
        });

        return Ok(data);
    }

    private static string GetArabicMonthName(int month)
    {
        return month switch
        {
            1 => "يناير",
            2 => "فبراير",
            3 => "مارس",
            4 => "أبريل",
            5 => "مايو",
            6 => "يونيو",
            7 => "يوليو",
            8 => "أغسطس",
            9 => "سبتمبر",
            10 => "أكتوبر",
            11 => "نوفمبر",
            12 => "ديسمبر",
            _ => ""
        };
    }
}
