using ErpSystem.Application.Abstractions.Auditing;
using ErpSystem.Domain.Common.Services;
using ErpSystem.Infrastructure.Persistence;
using ErpSystem.Infrastructure.Persistence.Auditing;

namespace ErpSystem.Infrastructure.Services;

public sealed class AuditService : IAuditService
{
    private readonly ErpDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AuditService(ErpDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task LogAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = entry.UserId,
            UserName = entry.UserName,
            ActionType = entry.ActionType,
            EntityName = entry.EntityName,
            EntityId = entry.EntityId,
            OldValues = entry.OldValues,
            NewValues = entry.NewValues,
            Timestamp = _dateTimeProvider.UtcNow,
            IpAddress = entry.IpAddress
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
