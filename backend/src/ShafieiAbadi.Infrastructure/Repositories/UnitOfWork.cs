using System.Collections.Concurrent;
using ShafieiAbadi.Application.Interfaces;
using ShafieiAbadi.Domain.Common;
using ShafieiAbadi.Infrastructure.Data;

namespace ShafieiAbadi.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation coordinating EF Core persistence and repository caching.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly ConcurrentDictionary<Type, object> _repositories = new();

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IGenericRepository<T> Repository<T>() where T : BaseEntity
    {
        return (IGenericRepository<T>)_repositories.GetOrAdd(typeof(T), _ => new GenericRepository<T>(_context));
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
