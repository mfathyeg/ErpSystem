using System.Text.Json;
using ErpSystem.Application.Abstractions.Outbox;
using ErpSystem.Domain.Common.Services;
using ErpSystem.Infrastructure.Persistence;
using ErpSystem.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;
using OutboxMessageEntity = ErpSystem.Infrastructure.Persistence.Outbox.OutboxMessage;
using OutboxMessageStatus = ErpSystem.Infrastructure.Persistence.Outbox.OutboxMessageStatus;

namespace ErpSystem.Infrastructure.Services;

public sealed class OutboxService : IOutboxService
{
    private readonly ErpDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public OutboxService(ErpDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task AddAsync(IIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        var outboxMessage = new OutboxMessageEntity
        {
            Id = @event.EventId,
            EventType = @event.EventType,
            Payload = JsonSerializer.Serialize(@event, @event.GetType()),
            OccurredOn = @event.OccurredOn,
            Status = OutboxMessageStatus.Pending
        };

        _context.OutboxMessages.Add(outboxMessage);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Application.Abstractions.Outbox.OutboxMessage>> GetUnprocessedAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var messages = await _context.OutboxMessages
            .Where(x => x.Status == OutboxMessageStatus.Pending)
            .OrderBy(x => x.OccurredOn)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        return messages.Select(x => new Application.Abstractions.Outbox.OutboxMessage(
            x.Id,
            x.EventType,
            x.Payload,
            x.OccurredOn,
            x.ProcessedOn,
            x.Status)).ToList();
    }

    public async Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        var message = await _context.OutboxMessages.FindAsync(new object[] { messageId }, cancellationToken);

        if (message is not null)
        {
            message.Status = OutboxMessageStatus.Processed;
            message.ProcessedOn = _dateTimeProvider.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task MarkAsFailedAsync(Guid messageId, string error, CancellationToken cancellationToken = default)
    {
        var message = await _context.OutboxMessages.FindAsync(new object[] { messageId }, cancellationToken);

        if (message is not null)
        {
            message.Status = OutboxMessageStatus.Failed;
            message.Error = error;
            message.RetryCount++;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
