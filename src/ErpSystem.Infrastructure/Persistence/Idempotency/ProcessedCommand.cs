namespace ErpSystem.Infrastructure.Persistence.Idempotency;

public class ProcessedCommand
{
    public Guid Id { get; set; }
    public Guid CommandId { get; set; }
    public Guid? UserId { get; set; }
    public DateTime ProcessedAt { get; set; }
}
