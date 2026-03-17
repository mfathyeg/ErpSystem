using ErpSystem.Application.Abstractions.Idempotency;
using ErpSystem.Domain.Common.Services;
using ErpSystem.Infrastructure.Persistence;
using ErpSystem.Infrastructure.Persistence.Idempotency;
using Microsoft.EntityFrameworkCore;

namespace ErpSystem.Infrastructure.Services;

public sealed class IdempotencyService : IIdempotencyService
{
    private readonly ErpDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public IdempotencyService(
        ErpDbContext context,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<bool> IsProcessedAsync(Guid commandId, CancellationToken cancellationToken = default)
    {
        return await _context.ProcessedCommands
            .AnyAsync(x => x.CommandId == commandId, cancellationToken);
    }

    public async Task MarkAsProcessedAsync(Guid commandId, CancellationToken cancellationToken = default)
    {
        var processedCommand = new ProcessedCommand
        {
            Id = Guid.NewGuid(),
            CommandId = commandId,
            UserId = _currentUserService.UserId,
            ProcessedAt = _dateTimeProvider.UtcNow
        };

        _context.ProcessedCommands.Add(processedCommand);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
