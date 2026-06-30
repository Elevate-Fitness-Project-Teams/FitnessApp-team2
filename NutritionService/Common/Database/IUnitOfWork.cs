using NutritionService.Models;

namespace NutritionService.Common.Database;

public interface IUnitOfWork : IAsyncDisposable
{
    Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default);
    Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
