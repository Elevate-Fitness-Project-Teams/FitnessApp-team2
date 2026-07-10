namespace FitnessCalculationService.Persistence.Repositories;

public interface IGenericRepository<T> where T : class
{
    IQueryable<T> GetQueryable();
}
