namespace ErpSystem.Application.Abstractions.Idempotency;

public interface IIdempotencyService
{
    Task<bool> IsProcessedAsync(Guid commandId, CancellationToken cancellationToken = default);
    Task MarkAsProcessedAsync(Guid commandId, CancellationToken cancellationToken = default);
}
