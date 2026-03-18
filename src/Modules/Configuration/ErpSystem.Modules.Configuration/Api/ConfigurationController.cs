using System.Security.Claims;
using Asp.Versioning;
using ErpSystem.Modules.Configuration.Domain.Entities;
using ErpSystem.Modules.Configuration.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpSystem.Modules.Configuration.Api;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ConfigurationController : ControllerBase
{
    private readonly ConfigurationDbContext _context;

    public ConfigurationController(ConfigurationDbContext context)
    {
        _context = context;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    [HttpGet("company")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCompanySettings()
    {
        var settings = await _context.CompanySettings.FirstOrDefaultAsync();

        if (settings == null)
        {
            return Ok(new CompanySettingsDto());
        }

        return Ok(new CompanySettingsDto
        {
            Id = settings.Id.ToString(),
            CompanyName = settings.CompanyName,
            Address = settings.Address,
            Phone = settings.Phone,
            Email = settings.Email,
            Website = settings.Website,
            TaxId = settings.TaxId,
            Currency = settings.Currency,
            Timezone = settings.Timezone,
            DateFormat = settings.DateFormat,
            LogoUrl = settings.LogoUrl
        });
    }

    [HttpPut("company")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateCompanySettings([FromBody] UpdateCompanySettingsRequest request)
    {
        var settings = await _context.CompanySettings.FirstOrDefaultAsync();

        if (settings == null)
        {
            settings = CompanySettings.Create(
                request.CompanyName,
                request.Address,
                request.Phone,
                request.Email,
                request.Currency,
                request.Timezone,
                request.DateFormat);

            _context.CompanySettings.Add(settings);
        }
        else
        {
            settings.Update(
                request.CompanyName,
                request.Address,
                request.Phone,
                request.Email,
                request.Website,
                request.TaxId,
                request.Currency,
                request.Timezone,
                request.DateFormat);
        }

        await _context.SaveChangesAsync();

        return Ok(new CompanySettingsDto
        {
            Id = settings.Id.ToString(),
            CompanyName = settings.CompanyName,
            Address = settings.Address,
            Phone = settings.Phone,
            Email = settings.Email,
            Website = settings.Website,
            TaxId = settings.TaxId,
            Currency = settings.Currency,
            Timezone = settings.Timezone,
            DateFormat = settings.DateFormat,
            LogoUrl = settings.LogoUrl
        });
    }

    [HttpGet("system")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSystemConfigs()
    {
        var configs = await _context.SystemConfigs.ToListAsync();

        var data = configs.Select(c => new SystemConfigDto
        {
            Id = c.Id.ToString(),
            Key = c.Key,
            Value = c.Value,
            Category = c.Category,
            Description = c.Description,
            IsEditable = c.IsEditable,
            DataType = c.DataType
        }).ToList();

        return Ok(data);
    }

    [HttpGet("system/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSystemConfig(string id)
    {
        if (!Guid.TryParse(id, out var configId))
            return NotFound(new { message = "الإعداد غير موجود" });

        var config = await _context.SystemConfigs.FindAsync(configId);
        if (config == null)
            return NotFound(new { message = "الإعداد غير موجود" });

        return Ok(new SystemConfigDto
        {
            Id = config.Id.ToString(),
            Key = config.Key,
            Value = config.Value,
            Category = config.Category,
            Description = config.Description,
            IsEditable = config.IsEditable,
            DataType = config.DataType
        });
    }

    [HttpPut("system/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSystemConfig(string id, [FromBody] UpdateConfigRequest request)
    {
        if (!Guid.TryParse(id, out var configId))
            return NotFound(new { message = "الإعداد غير موجود" });

        var config = await _context.SystemConfigs.FindAsync(configId);
        if (config == null)
            return NotFound(new { message = "الإعداد غير موجود" });

        try
        {
            config.UpdateValue(request.Value);
            await _context.SaveChangesAsync();

            return Ok(new SystemConfigDto
            {
                Id = config.Id.ToString(),
                Key = config.Key,
                Value = config.Value,
                Category = config.Category,
                Description = config.Description,
                IsEditable = config.IsEditable,
                DataType = config.DataType
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("system")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSystemConfig([FromBody] CreateSystemConfigRequest request)
    {
        var existingConfig = await _context.SystemConfigs.FirstOrDefaultAsync(c => c.Key == request.Key);
        if (existingConfig != null)
            return BadRequest(new { message = "مفتاح الإعداد موجود بالفعل" });

        var config = SystemConfig.Create(
            request.Key,
            request.Value,
            request.Category,
            request.Description,
            request.IsEditable,
            request.DataType);

        _context.SystemConfigs.Add(config);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSystemConfig), new { id = config.Id }, new SystemConfigDto
        {
            Id = config.Id.ToString(),
            Key = config.Key,
            Value = config.Value,
            Category = config.Category,
            Description = config.Description,
            IsEditable = config.IsEditable,
            DataType = config.DataType
        });
    }

    [HttpGet("notifications")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotificationPrefs()
    {
        var currentUserId = GetCurrentUserId();
        var prefs = await _context.UserNotificationPrefs
            .FirstOrDefaultAsync(p => p.UserId == currentUserId);

        if (prefs == null)
        {
            return Ok(new NotificationPrefsDto
            {
                EmailNotifications = true,
                PushNotifications = true,
                OrderUpdates = true,
                InventoryAlerts = true,
                SystemAlerts = true
            });
        }

        return Ok(new NotificationPrefsDto
        {
            EmailNotifications = prefs.EmailNotifications,
            PushNotifications = prefs.PushNotifications,
            OrderUpdates = prefs.OrderUpdates,
            InventoryAlerts = prefs.InventoryAlerts,
            SystemAlerts = prefs.SystemAlerts
        });
    }

    [HttpPut("notifications")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateNotificationPrefs([FromBody] NotificationPrefsDto request)
    {
        var currentUserId = GetCurrentUserId();
        var prefs = await _context.UserNotificationPrefs
            .FirstOrDefaultAsync(p => p.UserId == currentUserId);

        if (prefs == null)
        {
            prefs = UserNotificationPrefs.Create(currentUserId);
            _context.UserNotificationPrefs.Add(prefs);
        }

        prefs.Update(
            request.EmailNotifications,
            request.PushNotifications,
            request.OrderUpdates,
            request.InventoryAlerts,
            request.SystemAlerts);

        await _context.SaveChangesAsync();

        return Ok(new NotificationPrefsDto
        {
            EmailNotifications = prefs.EmailNotifications,
            PushNotifications = prefs.PushNotifications,
            OrderUpdates = prefs.OrderUpdates,
            InventoryAlerts = prefs.InventoryAlerts,
            SystemAlerts = prefs.SystemAlerts
        });
    }
}

public class CompanySettingsDto
{
    public string? Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Currency { get; set; } = "SAR";
    public string Timezone { get; set; } = "Asia/Riyadh";
    public string DateFormat { get; set; } = "DD/MM/YYYY";
    public string? LogoUrl { get; set; }
}

public class SystemConfigDto
{
    public string Id { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEditable { get; set; }
    public string? DataType { get; set; }
}

public class NotificationPrefsDto
{
    public bool EmailNotifications { get; set; }
    public bool PushNotifications { get; set; }
    public bool OrderUpdates { get; set; }
    public bool InventoryAlerts { get; set; }
    public bool SystemAlerts { get; set; }
}

public record UpdateCompanySettingsRequest(
    string CompanyName,
    string Address,
    string Phone,
    string Email,
    string? Website,
    string? TaxId,
    string Currency,
    string Timezone,
    string DateFormat);

public record UpdateConfigRequest(string Value);

public record CreateSystemConfigRequest(
    string Key,
    string Value,
    string Category,
    string Description,
    bool IsEditable = true,
    string? DataType = "string");
