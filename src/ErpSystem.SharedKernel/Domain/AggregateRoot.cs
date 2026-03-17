namespace ErpSystem.SharedKernel.Domain;

public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    public int Version { get; protected set; }

    protected AggregateRoot() : base() { }

    protected AggregateRoot(TId id) : base(id) { }

    public void IncrementVersion()
    {
        Version++;
    }
}
