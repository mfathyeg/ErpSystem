using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ErpSystem.Modules.Notifications.Api;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private static readonly List<NotificationDto> _notifications = GenerateMockNotifications();

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetNotifications(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? type = null,
        [FromQuery] bool? isRead = null)
    {
        var query = _notifications.AsQueryable();

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(n => n.Type.Equals(type, StringComparison.OrdinalIgnoreCase));
        }

        if (isRead.HasValue)
        {
            query = query.Where(n => n.IsRead == isRead.Value);
        }

        query = query.OrderByDescending(n => n.CreatedAt);

        var totalCount = query.Count();
        var data = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return Ok(new PaginatedResponse<NotificationDto>
        {
            Data = data,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    [HttpGet("unread-count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetUnreadCount()
    {
        var count = _notifications.Count(n => !n.IsRead);
        return Ok(count);
    }

    [HttpPut("{id:int}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult MarkAsRead(int id)
    {
        var notification = _notifications.FirstOrDefault(n => n.Id == id);
        if (notification == null)
            return NotFound(new { message = "الإشعار غير موجود" });

        notification.IsRead = true;
        return Ok(notification);
    }

    [HttpPut("mark-all-read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult MarkAllAsRead()
    {
        foreach (var notification in _notifications)
        {
            notification.IsRead = true;
        }
        return Ok(new { message = "تم تحديد جميع الإشعارات كمقروءة" });
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteNotification(int id)
    {
        var notification = _notifications.FirstOrDefault(n => n.Id == id);
        if (notification == null)
            return NotFound(new { message = "الإشعار غير موجود" });

        _notifications.Remove(notification);
        return NoContent();
    }

    private static List<NotificationDto> GenerateMockNotifications()
    {
        var types = new[] { "Info", "Success", "Warning", "Error", "Order", "Inventory", "System" };
        var messages = new[]
        {
            ("طلب جديد", "تم استلام طلب جديد #ORD-2024-001", "Order"),
            ("تنبيه مخزون", "المخزون منخفض: شاشة 27 بوصة", "Warning"),
            ("دفعة مستلمة", "تم استلام دفعة بقيمة 1,250.00 ر.س", "Success"),
            ("تحديث النظام", "تم تحديث النظام بنجاح", "System"),
            ("مستخدم جديد", "تم تسجيل مستخدم جديد", "Info"),
            ("طلب ملغي", "تم إلغاء الطلب #ORD-2024-005", "Error"),
            ("شحنة جاهزة", "الشحنة جاهزة للتسليم", "Order"),
            ("تجديد اشتراك", "يرجى تجديد الاشتراك قبل نهاية الشهر", "Warning")
        };

        var notifications = new List<NotificationDto>();
        var random = new Random(42);

        for (int i = 1; i <= 20; i++)
        {
            var message = messages[random.Next(messages.Length)];
            notifications.Add(new NotificationDto
            {
                Id = i,
                UserId = 1,
                Title = message.Item1,
                Message = message.Item2,
                Type = message.Item3,
                IsRead = random.Next(10) > 4,
                CreatedAt = DateTime.UtcNow.AddHours(-random.Next(1, 168))
            });
        }

        return notifications;
    }
}

public class PaginatedResponse<T>
{
    public List<T> Data { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class NotificationDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
