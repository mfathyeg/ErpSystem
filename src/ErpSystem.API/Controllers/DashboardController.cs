using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErpSystem.API.Controllers;

[ApiController]
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetStats()
    {
        return Ok(new
        {
            TotalRevenue = 125750.00m,
            TotalOrders = 1284,
            TotalProducts = 856,
            TotalCustomers = 2847,
            RevenueGrowth = 12.5m,
            OrdersGrowth = 8.3m,
            ProductsGrowth = 5.2m,
            CustomersGrowth = 15.7m
        });
    }

    [HttpGet("recent-orders")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetRecentOrders()
    {
        var orders = new[]
        {
            new { Id = 1, OrderNumber = "ORD-2024-001", CustomerName = "أحمد محمد", Status = "Pending", TotalAmount = 1250.00m, OrderDate = DateTime.UtcNow.AddHours(-2) },
            new { Id = 2, OrderNumber = "ORD-2024-002", CustomerName = "سارة علي", Status = "Processing", TotalAmount = 890.50m, OrderDate = DateTime.UtcNow.AddHours(-5) },
            new { Id = 3, OrderNumber = "ORD-2024-003", CustomerName = "خالد عبدالله", Status = "Shipped", TotalAmount = 2100.00m, OrderDate = DateTime.UtcNow.AddHours(-8) },
            new { Id = 4, OrderNumber = "ORD-2024-004", CustomerName = "نورة سعيد", Status = "Delivered", TotalAmount = 450.75m, OrderDate = DateTime.UtcNow.AddDays(-1) },
            new { Id = 5, OrderNumber = "ORD-2024-005", CustomerName = "فهد العتيبي", Status = "Pending", TotalAmount = 3200.00m, OrderDate = DateTime.UtcNow.AddDays(-1) }
        };

        return Ok(orders);
    }

    [HttpGet("top-products")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetTopProducts()
    {
        var products = new[]
        {
            new { Id = 1, Name = "شاشة كمبيوتر 27 بوصة", Category = "Electronics", Sales = 156, Revenue = 234000.00m },
            new { Id = 2, Name = "كرسي مكتب مريح", Category = "Furniture", Sales = 89, Revenue = 75650.00m },
            new { Id = 3, Name = "لوحة مفاتيح لاسلكية", Category = "Accessories", Sales = 234, Revenue = 58500.00m },
            new { Id = 4, Name = "ماوس احترافي", Category = "Accessories", Sales = 312, Revenue = 56160.00m },
            new { Id = 5, Name = "سماعات رأس", Category = "Accessories", Sales = 145, Revenue = 65250.00m }
        };

        return Ok(products);
    }

    [HttpGet("revenue-chart")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetRevenueChart()
    {
        var data = new[]
        {
            new { Month = "يناير", Revenue = 45000.00m, Orders = 120 },
            new { Month = "فبراير", Revenue = 52000.00m, Orders = 145 },
            new { Month = "مارس", Revenue = 48000.00m, Orders = 132 },
            new { Month = "أبريل", Revenue = 61000.00m, Orders = 178 },
            new { Month = "مايو", Revenue = 55000.00m, Orders = 156 },
            new { Month = "يونيو", Revenue = 67000.00m, Orders = 189 }
        };

        return Ok(data);
    }

    [HttpGet("team-members")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetTeamMembers()
    {
        var members = new[]
        {
            new { Id = 1, Name = "أحمد محمد", Role = "مدير", Avatar = "A", Status = "online", TasksCompleted = 45 },
            new { Id = 2, Name = "سارة علي", Role = "مشرف", Avatar = "S", Status = "online", TasksCompleted = 38 },
            new { Id = 3, Name = "خالد عبدالله", Role = "موظف", Avatar = "K", Status = "away", TasksCompleted = 32 },
            new { Id = 4, Name = "نورة سعيد", Role = "موظف", Avatar = "N", Status = "offline", TasksCompleted = 28 }
        };

        return Ok(members);
    }

    [HttpGet("activity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetRecentActivity()
    {
        var activities = new[]
        {
            new { Id = 1, User = "أحمد محمد", Action = "أنشأ طلب جديد", Target = "#ORD-2024-001", Time = DateTime.UtcNow.AddMinutes(-15) },
            new { Id = 2, User = "سارة علي", Action = "حدّث حالة الطلب", Target = "#ORD-2024-002", Time = DateTime.UtcNow.AddMinutes(-30) },
            new { Id = 3, User = "خالد عبدالله", Action = "أضاف منتج جديد", Target = "شاشة 32 بوصة", Time = DateTime.UtcNow.AddHours(-1) },
            new { Id = 4, User = "نورة سعيد", Action = "أكمل عملية الشحن", Target = "#ORD-2024-003", Time = DateTime.UtcNow.AddHours(-2) },
            new { Id = 5, User = "أحمد محمد", Action = "استلم دفعة", Target = "1,250.00 ر.س", Time = DateTime.UtcNow.AddHours(-3) }
        };

        return Ok(activities);
    }
}
