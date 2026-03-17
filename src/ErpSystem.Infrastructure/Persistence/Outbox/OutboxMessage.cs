namespace ErpSystem.Infrastructure.Persistence.Outbox;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime OccurredOn { get; set; }
    public DateTime? ProcessedOn { get; set; }
    public string Status { get; set; } = OutboxMessageStatus.Pending;
    public string? Error { get; set; }
    public int RetryCount { get; set; }
}

public static class OutboxMessageStatus
{
    public const string Pending = "Pending";
    public const string Processed = "Processed";
    public const string Failed = "Failed";
}
