namespace ErpSystem.SharedKernel.Domain;

public abstract record StronglyTypedId<TValue>(TValue Value) where TValue : notnull
{
    public override string ToString() => Value.ToString()!;
}

public abstract record EntityId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public override string ToString() => Value.ToString();
}
