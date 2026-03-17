using System.Linq.Expressions;
using ErpSystem.Domain.Common.Repositories;
using ErpSystem.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;

namespace ErpSystem.Infrastructure.Persistence.Repositories;

public abstract class Repository<TEntity, TId> : IRepository<TEntity, TId>
    where TEntity : AggregateRoot<TId>
    where TId : notnull
{
    protected readonly DbContext Context;
    protected readonly DbSet<TEntity> DbSet;

    protected Repository(DbContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default)
    {
        var entity = await DbSet.FindAsync(new object[] { id }, cancellationToken);
        return entity is not null;
    }

    public virtual void Add(TEntity entity)
    {
        DbSet.Add(entity);
    }

    public virtual void Update(TEntity entity)
    {
        DbSet.Update(entity);
    }

    public virtual void Remove(TEntity entity)
    {
        DbSet.Remove(entity);
    }
}
