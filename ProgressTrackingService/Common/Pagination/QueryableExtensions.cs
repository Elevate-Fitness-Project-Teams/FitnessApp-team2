using Microsoft.EntityFrameworkCore;

namespace ProgressTrackingService.Common.Pagination;

public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : (pageSize > 100 ? 100 : pageSize);

        var totalCount = await query.CountAsync(cancellationToken);
        if (totalCount == 0)
        {
            return PagedResult<T>.Create(Array.Empty<T>(), 0, page, pageSize);
        }

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<T>.Create(items, totalCount, page, pageSize);
    }
}
