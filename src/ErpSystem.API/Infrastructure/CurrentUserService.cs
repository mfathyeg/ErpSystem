using System.Security.Claims;
using ErpSystem.Domain.Common.Services;

namespace ErpSystem.API.Infrastructure;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public string? UserName =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value
        ?? _httpContextAccessor.HttpContext?.User?.FindFirst("name")?.Value;

    public string? Email =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value
        ?? _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value;

    public IReadOnlyCollection<string> Roles =>
        _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList()
            .AsReadOnly()
        ?? Array.Empty<string>().AsReadOnly();

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public string? IpAddress =>
        _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
}
