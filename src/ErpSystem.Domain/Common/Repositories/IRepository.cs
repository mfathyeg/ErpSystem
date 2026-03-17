using System.Linq.Expressions;
using ErpSystem.SharedKernel.Domain;

namespace ErpSystem.Domain.Common.Repositories;

public interface IRepository<TEntity, TId>
    where TEntity : AggregateRoot<TId>
    where TId : notnull
{
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);
    void Add(TEntity entity);
    void Update(TEntity entity);
    void Remove(TEntity entity);
}
