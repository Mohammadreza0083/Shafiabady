using ShafieiAbadi.Domain.Common;

namespace ShafieiAbadi.Application.Interfaces;

/// <summary>
/// Unit of Work pattern interface coordinating transactions and entity repositories.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Obtains a generic repository instance for the specified entity type.
    /// </summary>
    /// <typeparam name="T">Domain entity type derived from BaseEntity.</typeparam>
    /// <returns>Repository instance.</returns>
    IGenericRepository<T> Repository<T>() where T : BaseEntity;

    /// <summary>
    /// Persists all pending entity changes to the underlying database within a single transaction.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
