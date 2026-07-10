using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Data;
using System.Linq.Expressions;

namespace ProgressTrackingService.Common.Database;

public class GeneralRepo<T> : IGeneralRepo<T> where T : class
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DbSet<T> _dbSet;

    public GeneralRepo(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<T>();
    }

    public async Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new[] { id }, cancellationToken);
    }

    public IQueryable<T> GetAll()
    {
        return _dbSet;
    }

    public IQueryable<T> Find(Expression<Func<T, bool>> predicate)
    {
        return _dbSet.Where(predicate);
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(predicate, cancellationToken);
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

    public void SaveInclude(T entity, params string[] includedProperties)
    {
        var keyProperties = _dbContext.Model.FindEntityType(typeof(T))!.FindPrimaryKey()!.Properties;

        var tracked = _dbSet.Local.FirstOrDefault(local =>
            keyProperties.All(p => p.PropertyInfo!.GetValue(local)!.Equals(p.PropertyInfo!.GetValue(entity))));

        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry;

        if (tracked == null)
        {
            entry = _dbContext.Entry(entity);
        }
        else
        {
            entry = _dbContext.Entry(tracked);
        }

        foreach (var property in entry.Properties)
        {
            if (property.Metadata.IsPrimaryKey())
                continue;
            else
            {
                if (includedProperties.Contains(property.Metadata.Name))
                {
                    property.IsModified = true;
                }
                else
                {
                    property.IsModified = false;
                }
            }
        }
    }
}