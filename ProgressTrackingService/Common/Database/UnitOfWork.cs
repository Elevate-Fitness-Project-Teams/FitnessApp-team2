using Microsoft.EntityFrameworkCore.Storage;
using ProgressTrackingService.Data;

namespace ProgressTrackingService.Common.Database;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Stack<string> _savepoints = new();
    private int _depth;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        var isOutermost = _depth == 0;

        if (isOutermost)
        {
            _transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        }
        else
        {
            var savepointName = $"SP_Depth{_depth}_{Guid.NewGuid().ToString("N")[..8]}";
            _savepoints.Push(savepointName);
            await _transaction!.CreateSavepointAsync(savepointName, cancellationToken);
        }

        _depth++;

        try
        {
            await action();

            _depth--;

            if (_depth == 0)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                await _transaction!.CommitAsync(cancellationToken);
            }
            else
            {
                _savepoints.Pop();
            }
        }
        catch
        {
            _depth--;

            if (_depth == 0)
            {
                await _transaction!.RollbackAsync(cancellationToken);
            }
            else if (_savepoints.Count > 0)
            {
                var savepoint = _savepoints.Pop();
                await _transaction!.RollbackToSavepointAsync(savepoint, cancellationToken);
            }

            throw;
        }
        finally
        {
            if (_depth == 0 && _transaction is not null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
                _savepoints.Clear();
            }
        }
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
    {
        T result = default!;
        await ExecuteAsync(async () => { result = await action(); }, cancellationToken);
        return result;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        GC.SuppressFinalize(this);
    }
}