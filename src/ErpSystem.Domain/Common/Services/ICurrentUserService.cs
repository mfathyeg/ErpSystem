namespace ErpSystem.Domain.Common.Services;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserName { get; }
    string? Email { get; }
    IReadOnlyCollection<string> Roles { get; }
    bool IsAuthenticated { get; }
    string? IpAddress { get; }
}
