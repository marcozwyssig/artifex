using Maestro.Shared.Domain.Specifications;

namespace Maestro.Shared.Domain;

/// <summary>
/// Base interface for repositories
/// Repositories provide collection-like interface for aggregates
/// </summary>
public interface IRepository<TEntity,TId>
    where TEntity : Entity<TId> where TId : struct
{
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TEntity> SaveAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds entities matching the specification
    /// </summary>
    Task<IReadOnlyCollection<TEntity>> FindAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the first entity matching the specification, or null
    /// </summary>
    Task<TEntity?> FindFirstAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts entities matching the specification
    /// </summary>
    Task<int> CountAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);
}
