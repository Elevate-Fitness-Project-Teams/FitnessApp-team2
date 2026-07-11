using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FitnessCalculationService.Persistence.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly FceDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(FceDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public IQueryable<T> GetQueryable()
    {
        return _dbSet.AsQueryable();
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
        var LocalEntity = _dbSet.Local.FirstOrDefault(e => ((dynamic)e).Id == ((dynamic)entity).Id);
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry;

        if (LocalEntity == null)
        {
            entry = _context.Entry(entity);
        }
        else
        {
            entry = _context.ChangeTracker.Entries<T>().First(e => ((dynamic)e.Entity).Id == ((dynamic)entity).Id);
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
