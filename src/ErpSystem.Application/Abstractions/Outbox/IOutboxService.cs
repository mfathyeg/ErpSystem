using ErpSystem.SharedKernel.Domain;

namespace ErpSystem.Application.Abstractions.Outbox;

public interface IOutboxService
{
    Task AddAsync(IIntegrationEvent @event, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OutboxMessage>> GetUnprocessedAsync(int batchSize, CancellationToken cancellationToken = default);
    Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default);
    Task MarkAsFailedAsync(Guid messageId, string error, CancellationToken cancellationToken = default);
}

public record OutboxMessage(
    Guid Id,
    string EventType,
    string Payload,
    DateTime OccurredOn,
    DateTime? ProcessedOn,
    string Status);
