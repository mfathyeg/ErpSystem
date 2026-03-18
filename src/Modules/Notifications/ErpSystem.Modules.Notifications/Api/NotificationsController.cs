using System.Security.Claims;
using Asp.Versioning;
using ErpSystem.Modules.Notifications.Domain.Entities;
using ErpSystem.Modules.Notifications.Domain.ValueObjects;
using ErpSystem.Modules.Notifications.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpSystem.Modules.Notifications.Api;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly NotificationsDbContext _context;

    public NotificationsController(NotificationsDbContext context)
    {
        _context = context;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? type = null,
        [FromQuery] bool? isRead = null)
    {
        var currentUserId = GetCurrentUserId();

        var query = _context.Notifications
            .Where(n => n.UserId == currentUserId)
            .AsQueryable();

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(n => n.Type.Code == type);
        }

        if (isRead.HasValue)
        {
            query = query.Where(n => n.IsRead == isRead.Value);
        }

        var totalCount = await query.CountAsync();

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var data = notifications.Select(n => new NotificationDto
        {
            Id = n.Id.ToString(),
            UserId = n.UserId.ToString(),
            Title = n.Title,
            Message = n.Message,
            Type = n.Type.Code,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt,
            ReadAt = n.ReadAt
        }).ToList();

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
    public async Task<IActionResult> GetUnreadCount()
    {
        var currentUserId = GetCurrentUserId();
        var count = await _context.Notifications
            .CountAsync(n => n.UserId == currentUserId && !n.IsRead);

        return Ok(count);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNotification(string id)
    {
        if (!Guid.TryParse(id, out var notificationId))
            return NotFound(new { message = "الإشعار غير موجود" });

        var currentUserId = GetCurrentUserId();
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == currentUserId);

        if (notification == null)
            return NotFound(new { message = "الإشعار غير موجود" });

        return Ok(new NotificationDto
        {
            Id = notification.Id.ToString(),
            UserId = notification.UserId.ToString(),
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type.Code,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
            ReadAt = notification.ReadAt
        });
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationRequest request)
    {
        var type = NotificationType.Create(request.Type);

        var notification = Notification.Create(
            request.UserId,
            request.Title,
            request.Message,
            type,
            request.RelatedEntityId,
            request.RelatedEntityType);

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, new NotificationDto
        {
            Id = notification.Id.ToString(),
            UserId = notification.UserId.ToString(),
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type.Code,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        });
    }

    [HttpPut("{id}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(string id)
    {
        if (!Guid.TryParse(id, out var notificationId))
            return NotFound(new { message = "الإشعار غير موجود" });

        var currentUserId = GetCurrentUserId();
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == currentUserId);

        if (notification == null)
            return NotFound(new { message = "الإشعار غير موجود" });

        notification.MarkAsRead();
        await _context.SaveChangesAsync();

        return Ok(new NotificationDto
        {
            Id = notification.Id.ToString(),
            UserId = notification.UserId.ToString(),
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type.Code,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
            ReadAt = notification.ReadAt
        });
    }

    [HttpPut("mark-all-read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var currentUserId = GetCurrentUserId();
        var unreadNotifications = await _context.Notifications
            .Where(n => n.UserId == currentUserId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in unreadNotifications)
        {
            notification.MarkAsRead();
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "تم تحديد جميع الإشعارات كمقروءة", count = unreadNotifications.Count });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteNotification(string id)
    {
        if (!Guid.TryParse(id, out var notificationId))
            return NotFound(new { message = "الإشعار غير موجود" });

        var currentUserId = GetCurrentUserId();
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == currentUserId);

        if (notification == null)
            return NotFound(new { message = "الإشعار غير موجود" });

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("clear-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearAllNotifications()
    {
        var currentUserId = GetCurrentUserId();
        var notifications = await _context.Notifications
            .Where(n => n.UserId == currentUserId)
            .ToListAsync();

        _context.Notifications.RemoveRange(notifications);
        await _context.SaveChangesAsync();

        return Ok(new { message = "تم حذف جميع الإشعارات", count = notifications.Count });
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
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

public record CreateNotificationRequest(
    Guid UserId,
    string Title,
    string Message,
    string Type,
    Guid? RelatedEntityId = null,
    string? RelatedEntityType = null);
