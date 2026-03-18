using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ErpSystem.Modules.Configuration.Api;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ConfigurationController : ControllerBase
{
    private static CompanySettingsDto _companySettings = GetDefaultCompanySettings();
    private static readonly List<SystemConfigDto> _systemConfigs = GenerateMockSystemConfigs();
    private static NotificationPrefsDto _notificationPrefs = GetDefaultNotificationPrefs();

    [HttpGet("company")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetCompanySettings()
    {
        return Ok(_companySettings);
    }

    [HttpPut("company")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult UpdateCompanySettings([FromBody] CompanySettingsDto settings)
    {
        _companySettings = settings;
        return Ok(_companySettings);
    }

    [HttpGet("system")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetSystemConfigs()
    {
        return Ok(_systemConfigs);
    }

    [HttpPut("system/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UpdateSystemConfig(int id, [FromBody] UpdateConfigRequest request)
    {
        var config = _systemConfigs.FirstOrDefault(c => c.Id == id);
        if (config == null)
            return NotFound(new { message = "الإعداد غير موجود" });

        if (config.IsEditable)
        {
            config.Value = request.Value;
        }

        return Ok(config);
    }

    [HttpGet("notifications")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetNotificationPrefs()
    {
        return Ok(_notificationPrefs);
    }

    [HttpPut("notifications")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult UpdateNotificationPrefs([FromBody] NotificationPrefsDto prefs)
    {
        _notificationPrefs = prefs;
        return Ok(_notificationPrefs);
    }

    private static CompanySettingsDto GetDefaultCompanySettings()
    {
        return new CompanySettingsDto
        {
            CompanyName = "شركة دورالوكس",
            Address = "شارع الملك فهد، الرياض، المملكة العربية السعودية",
            Phone = "+966 11 234 5678",
            Email = "info@duralux.sa",
            Website = "https://duralux.sa",
            TaxId = "300123456789003",
            Currency = "SAR",
            Timezone = "Asia/Riyadh",
            DateFormat = "DD/MM/YYYY"
        };
    }

    private static List<SystemConfigDto> GenerateMockSystemConfigs()
    {
        return new List<SystemConfigDto>
        {
            new() { Id = 1, Key = "MAX_LOGIN_ATTEMPTS", Value = "5", Category = "Security", Description = "الحد الأقصى لمحاولات تسجيل الدخول قبل القفل", IsEditable = true },
            new() { Id = 2, Key = "SESSION_TIMEOUT", Value = "30", Category = "Security", Description = "مهلة انتهاء الجلسة بالدقائق", IsEditable = true },
            new() { Id = 3, Key = "LOW_STOCK_THRESHOLD", Value = "10", Category = "Inventory", Description = "حد التنبيه الافتراضي للمخزون المنخفض", IsEditable = true },
            new() { Id = 4, Key = "ORDER_PREFIX", Value = "ORD", Category = "Orders", Description = "بادئة رقم الطلب", IsEditable = true },
            new() { Id = 5, Key = "TAX_RATE", Value = "15", Category = "Finance", Description = "نسبة ضريبة القيمة المضافة", IsEditable = true }
        };
    }

    private static NotificationPrefsDto GetDefaultNotificationPrefs()
    {
        return new NotificationPrefsDto
        {
            EmailNotifications = true,
            PushNotifications = true,
            OrderUpdates = true,
            InventoryAlerts = true,
            SystemAlerts = true
        };
    }
}

public class CompanySettingsDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
    public string DateFormat { get; set; } = string.Empty;
}

public class SystemConfigDto
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEditable { get; set; }
}

public class NotificationPrefsDto
{
    public bool EmailNotifications { get; set; }
    public bool PushNotifications { get; set; }
    public bool OrderUpdates { get; set; }
    public bool InventoryAlerts { get; set; }
    public bool SystemAlerts { get; set; }
}

public record UpdateConfigRequest(string Value);
