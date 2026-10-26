using MediatR;
using Microsoft.Extensions.Logging;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Caching;
using ProgressTrackingService.Common.Domain;
using ProgressTrackingService.Common.Errors;
using ProgressTrackingService.Features.Progress.Queries.ViewUserProgress;
using ProgressTrackingService.Features.Streaks.Queries.GetStreak;

namespace ProgressTrackingService.Features.Progress.Queries.GetProgressStats;

public record GetProgressStatsQuery(Guid UserId) : IRequest<Result<ProgressStatsDto>>;

public class GetProgressStatsQueryHandler : IRequestHandler<GetProgressStatsQuery, Result<ProgressStatsDto>>
{
    private readonly IMediator _mediator;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetProgressStatsQueryHandler> _logger;

    public GetProgressStatsQueryHandler(
        IMediator mediator,
        ICacheService cacheService,
        ILogger<GetProgressStatsQueryHandler> logger)
    {
        _mediator = mediator;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<ProgressStatsDto>> Handle(GetProgressStatsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = ProgressCacheKeys.Stats(request.UserId);

        try
        {
            var cached = await _cacheService.GetAsync<ProgressStatsDto>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for user progress stats: {UserId}", request.UserId);
                return Result.Success(cached);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read progress stats from cache for user {UserId}. Falling back to queries.", request.UserId);
        }

        // Check if user exists in the system
        var existsResult = await _mediator.Send(new CheckUserExistsQuery(request.UserId), cancellationToken);
        if (!existsResult.IsSuccess || !existsResult.Value)
        {
            return Result.Failure<ProgressStatsDto>(
                Error.NotFound(ProgressErrors.ResUserNotFound, "User not found."));
        }

        // Fetch statistics (0 totals if row not yet initialized)
        var statsResult = await _mediator.Send(new GetUserStatisticsQuery(request.UserId), cancellationToken);
        var stats = statsResult.IsSuccess ? statsResult.Value : null;

        // Fetch streak information
        var streakResult = await _mediator.Send(new GetStreakQuery(request.UserId), cancellationToken);
        var streak = streakResult.IsSuccess ? streakResult.Value : null;

        // Compute effective current streak based on calendar day decay
        var effectiveStreak = StreakCalculator.EffectiveCurrentStreak(
            streak?.CurrentStreak ?? 0,
            streak?.LastWorkoutDate,
            DateTime.UtcNow);

        var dto = new ProgressStatsDto
        {
            TotalWorkouts = stats?.TotalWorkouts ?? 0,
            TotalCaloriesBurned = stats?.TotalCaloriesBurned ?? 0,
            TotalMinutes = stats?.TotalMinutes ?? 0,
            TotalWeightLost = stats?.TotalWeightLost ?? 0,
            CurrentWeight = (stats != null && stats.CurrentWeight > 0) ? stats.CurrentWeight : null,
            CurrentStreak = effectiveStreak,
            LongestStreak = streak?.LongestStreak ?? 0,
            LastWorkoutDate = streak?.LastWorkoutDate
        };

        try
        {
            await _cacheService.SetAsync(cacheKey, dto, ProgressCacheKeys.StatsTtl, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write progress stats to cache for user {UserId}", request.UserId);
        }

        return Result.Success(dto);
    }
}
