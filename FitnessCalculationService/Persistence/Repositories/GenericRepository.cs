using Microsoft.EntityFrameworkCore;

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
}
