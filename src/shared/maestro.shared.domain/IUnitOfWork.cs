namespace Maestro.Shared.Domain;

/// <summary>
/// Unit of Work pattern - maintains a list of objects affected by a business transaction
/// and coordinates the writing out of changes
/// </summary>
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);
}
