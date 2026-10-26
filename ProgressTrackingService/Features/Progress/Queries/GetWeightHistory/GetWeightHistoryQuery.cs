using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Caching;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Common.Errors;
using ProgressTrackingService.Common.Pagination;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.Progress.Queries.GetWeightHistory;

public record GetWeightHistoryQuery(
    Guid UserId,
    DateTime? From = null,
    DateTime? To = null,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PagedResult<WeightHistoryDto>>>;

public class GetWeightHistoryQueryHandler : IRequestHandler<GetWeightHistoryQuery, Result<PagedResult<WeightHistoryDto>>>
{
    private readonly IGeneralRepo<WeightHistory> _weightHistoryRepo;
    private readonly ICacheService _cacheService;
    private readonly ICacheInvalidator _cacheInvalidator;
    private readonly ILogger<GetWeightHistoryQueryHandler> _logger;

    public GetWeightHistoryQueryHandler(
        IGeneralRepo<WeightHistory> weightHistoryRepo,
        ICacheService cacheService,
        ICacheInvalidator cacheInvalidator,
        ILogger<GetWeightHistoryQueryHandler> logger)
    {
        _weightHistoryRepo = weightHistoryRepo;
        _cacheService = cacheService;
        _cacheInvalidator = cacheInvalidator;
        _logger = logger;
    }

    public async Task<Result<PagedResult<WeightHistoryDto>>> Handle(GetWeightHistoryQuery request, CancellationToken cancellationToken)
    {
        if (request.From.HasValue && request.To.HasValue && request.From.Value > request.To.Value)
        {
            return Result.Failure<PagedResult<WeightHistoryDto>>(
                Error.Validation(ProgressErrors.ValInvalidDate, "From date cannot be after To date."));
        }

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : (request.PageSize > 100 ? 100 : request.PageSize);
        var isDateFiltered = request.From.HasValue || request.To.HasValue;

        string? cacheKey = null;

        // Cache-aside: Only cache default paged requests without custom date bounds
        if (!isDateFiltered)
        {
            try
            {
                var version = await _cacheInvalidator.GetUserVersionAsync(request.UserId, cancellationToken);
                cacheKey = ProgressCacheKeys.WeightHistory(request.UserId, version, page, pageSize);

                var cached = await _cacheService.GetAsync<PagedResult<WeightHistoryDto>>(cacheKey, cancellationToken);
                if (cached != null)
                {
                    _logger.LogDebug("Cache hit for weight history: {CacheKey}", cacheKey);
                    return Result.Success(cached);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read weight history cache for user {UserId}. Falling back to database.", request.UserId);
            }
        }

        var query = _weightHistoryRepo.Find(w => w.UserId == request.UserId).AsNoTracking();

        if (request.From.HasValue)
        {
            query = query.Where(w => w.Date >= request.From.Value);
        }

        if (request.To.HasValue)
        {
            query = query.Where(w => w.Date <= request.To.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Chronological order (ASC) as required by Story 5.5 for trend analysis
        var items = await query
            .OrderBy(w => w.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(wh => new WeightHistoryDto
            {
                Id = wh.Id,
                Weight = wh.Weight,
                Date = wh.Date,
                Notes = wh.Notes
            })
            .ToListAsync(cancellationToken);

        var pagedResult = PagedResult<WeightHistoryDto>.Create(items, totalCount, page, pageSize);

        if (!isDateFiltered && cacheKey != null)
        {
            try
            {
                await _cacheService.SetAsync(cacheKey, pagedResult, ProgressCacheKeys.WeightHistoryTtl, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to write weight history cache for key {CacheKey}", cacheKey);
            }
        }

        return Result.Success(pagedResult);
    }
}
