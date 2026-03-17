namespace ErpSystem.SharedKernel.Domain;

public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    string? CreatedBy { get; set; }
    DateTime? LastModifiedAt { get; set; }
    string? LastModifiedBy { get; set; }
}

public abstract class AuditableEntity<TId> : Entity<TId>, IAuditableEntity where TId : notnull
{
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    protected AuditableEntity() : base() { }
    protected AuditableEntity(TId id) : base(id) { }
}

public abstract class AuditableAggregateRoot<TId> : AggregateRoot<TId>, IAuditableEntity where TId : notnull
{
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    protected AuditableAggregateRoot() : base() { }
    protected AuditableAggregateRoot(TId id) : base(id) { }
}
