using System.Linq.Expressions;
using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Domain.Common;

namespace ShafieiAbadi.Application.Interfaces;

/// <summary>
/// Generic repository interface providing decoupled CRUD and query abstractions for domain entities.
/// </summary>
/// <typeparam name="T">Aggregate root or domain entity type.</typeparam>
public interface IGenericRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">Entity GUID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Entity instance or null if not found.</returns>
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Retrieves all active (non-deleted) entities.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of entities.</returns>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Searches for entities matching a specified predicate expression.
    /// </summary>
    /// <param name="predicate">Filter expression.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of matching entities.</returns>
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a paginated subset of entities matching criteria with optional sorting.
    /// </summary>
    /// <param name="pageNumber">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="filter">Optional filter expression.</param>
    /// <param name="orderBy">Optional sorting expression.</param>
    /// <param name="ascending">Sort direction flag.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <param name="includes">Navigation properties to eager-load.</param>
    /// <returns>PagedResult containing the current slice and total matching count.</returns>
    Task<PagedResult<T>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true,
        CancellationToken ct = default,
        params Expression<Func<T, object>>[] includes);

    /// <summary>
    /// Adds a new entity to the persistence store.
    /// </summary>
    /// <param name="entity">Entity instance.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<T> AddAsync(T entity, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing entity in the persistence store.
    /// </summary>
    /// <param name="entity">Entity instance.</param>
    void Update(T entity);

    /// <summary>
    /// Performs a soft deletion on an entity by marking its IsDeleted flag.
    /// </summary>
    /// <param name="entity">Entity instance.</param>
    void SoftDelete(T entity);

    /// <summary>
    /// Counts total entities matching an optional filter predicate.
    /// </summary>
    /// <param name="predicate">Optional filter expression.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Total matching count.</returns>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);
}
