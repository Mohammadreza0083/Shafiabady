using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ShafieiAbadi.Application.Interfaces;
using ShafieiAbadi.Domain.Common;
using ShafieiAbadi.Infrastructure.Data;

namespace ShafieiAbadi.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        string includeProperties = "",
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        foreach (var includeProperty in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty.Trim());
        }

        var totalCount = await query.CountAsync(cancellationToken);

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        var skip = (pageNumber - 1) * pageSize;
        var items = await query.Skip(skip).Take(pageSize).ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        if (predicate != null)
        {
            return await _dbSet.CountAsync(predicate, cancellationToken);
        }
        return await _dbSet.CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }
}
