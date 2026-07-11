namespace FitnessCalculationService.Persistence.Repositories;

public interface IGenericRepository<T> where T : class
{
    IQueryable<T> GetQueryable();
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Delete(T entity);
    void SaveInclude(T entity, params string[] includedProperties);
}
