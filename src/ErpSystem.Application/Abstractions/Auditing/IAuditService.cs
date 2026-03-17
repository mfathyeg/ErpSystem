namespace ErpSystem.Application.Abstractions.Auditing;

public interface IAuditService
{
    Task LogAsync(AuditEntry entry, CancellationToken cancellationToken = default);
}

public record AuditEntry(
    Guid UserId,
    string UserName,
    string ActionType,
    string EntityName,
    string EntityId,
    string? OldValues,
    string? NewValues,
    string? IpAddress);
