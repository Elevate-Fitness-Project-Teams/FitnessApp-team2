using MediatR;
using Microsoft.Extensions.Logging;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Caching;
using ProgressTrackingService.Common.Domain;
using ProgressTrackingService.Features.Achievements.DTOs;
using ProgressTrackingService.Features.Progress.Queries.GetWeightHistory;
using ProgressTrackingService.Features.Progress.Queries.ViewUserProgress;
using ProgressTrackingService.Features.Streaks.Queries.GetStreak;

namespace ProgressTrackingService.Features.Progress.Queries.GetProgressDashboard;

public record GetProgressDashboardQuery(
    Guid UserId,
    string? Period = "week",
    DateTime? StartDate = null,
    DateTime? EndDate = null) : IRequest<Result<ProgressDashboardResponse>>;

public class GetProgressDashboardQueryHandler : IRequestHandler<GetProgressDashboardQuery, Result<ProgressDashboardResponse>>
{
    private readonly IMediator _mediator;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetProgressDashboardQueryHandler> _logger;

    public GetProgressDashboardQueryHandler(
        IMediator mediator,
        ICacheService cacheService,
        ILogger<GetProgressDashboardQueryHandler> logger)
    {
        _mediator = mediator;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<ProgressDashboardResponse>> Handle(
        GetProgressDashboardQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Resolve date boundaries
        var resolutionResult = PeriodResolver.Resolve(
            request.Period,
            request.StartDate,
            request.EndDate,
            DateTime.UtcNow);

        if (!resolutionResult.IsSuccess)
        {
            return Result.Failure<ProgressDashboardResponse>(resolutionResult.Error);
        }

        var bounds = resolutionResult.Value;
        var isPreset = bounds.Period != "custom";
        string? cacheKey = null;

        // 2. Cache-aside for preset periods only (week, month, year, all)
        if (isPreset)
        {
            cacheKey = ProgressCacheKeys.Dashboard(request.UserId, bounds.Period);
            try
            {
                var cached = await _cacheService.GetAsync<ProgressDashboardResponse>(cacheKey, cancellationToken);
                if (cached != null)
                {
                    _logger.LogDebug("Cache hit for progress dashboard: {CacheKey}", cacheKey);
                    return Result.Success(cached);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read dashboard cache for user {UserId}", request.UserId);
            }
        }

        // 3. Sequentially execute queries on scoped DbContext
        // A. Current period workout aggregate
        var currentWorkoutAgg = await _mediator.Send(
            new GetWorkoutAggregateQuery(request.UserId, bounds.CurrentRange.From, bounds.CurrentRange.To),
            cancellationToken);

        // B. Previous period workout aggregate for comparison (if applicable)
        WorkoutAggregateDto? previousWorkoutAgg = null;
        if (bounds.PreviousRange != null)
        {
            var prevResult = await _mediator.Send(
                new GetWorkoutAggregateQuery(request.UserId, bounds.PreviousRange.From, bounds.PreviousRange.To),
                cancellationToken);
            if (prevResult.IsSuccess)
            {
                previousWorkoutAgg = prevResult.Value;
            }
        }

        // C. Weight change in current period
        var weightChangeResult = await _mediator.Send(
            new GetWeightChangeInRangeQuery(request.UserId, bounds.CurrentRange.From, bounds.CurrentRange.To),
            cancellationToken);

        // D. Weight history points in current range (up to 50 items)
        var weightHistoryResult = await _mediator.Send(
            new GetWeightHistoryQuery(
                request.UserId,
                From: bounds.CurrentRange.From,
                To: bounds.CurrentRange.To,
                Page: 1,
                PageSize: 50),
            cancellationToken);

        // E. Recent workout logs in current range (up to 50 items)
        var workoutHistoryResult = await _mediator.Send(
            new GetWorkoutLogsQuery(request.UserId, Limit: 50),
            cancellationToken);

        // Filter workouts to current range
        var workoutsInRange = (workoutHistoryResult.IsSuccess && workoutHistoryResult.Value != null)
            ? workoutHistoryResult.Value
                .Where(w => w.CompletedAt.Date >= bounds.CurrentRange.From.Date && w.CompletedAt.Date <= bounds.CurrentRange.To.Date)
                .ToList()
            : new List<WorkoutLogDto>();

        // F. Streak status
        var streakResult = await _mediator.Send(new GetStreakQuery(request.UserId), cancellationToken);
        var effectiveStreak = 0;
        var longestStreak = 0;
        if (streakResult.IsSuccess && streakResult.Value != null)
        {
            effectiveStreak = StreakCalculator.EffectiveCurrentStreak(
                streakResult.Value.CurrentStreak,
                streakResult.Value.LastWorkoutDate,
                DateTime.UtcNow);
            longestStreak = streakResult.Value.LongestStreak;
        }

        // G. Achievements unlocked in current range
        var achievementsResult = await _mediator.Send(
            new GetAchievementsEarnedInRangeQuery(request.UserId, bounds.CurrentRange.From, bounds.CurrentRange.To),
            cancellationToken);

        // 4. Compute comparisons (% change)
        var curWorkouts = currentWorkoutAgg.Value?.WorkoutsCount ?? 0;
        var curCalories = currentWorkoutAgg.Value?.TotalCalories ?? 0;
        var curMinutes = currentWorkoutAgg.Value?.TotalMinutes ?? 0;
        var avgRating = currentWorkoutAgg.Value?.AvgRating ?? 0.0;

        double? workoutsChangePct = null;
        double? caloriesChangePct = null;
        double? minutesChangePct = null;

        if (previousWorkoutAgg != null)
        {
            if (previousWorkoutAgg.WorkoutsCount > 0)
            {
                workoutsChangePct = Math.Round(((double)(curWorkouts - previousWorkoutAgg.WorkoutsCount) / previousWorkoutAgg.WorkoutsCount) * 100.0, 1);
            }
            if (previousWorkoutAgg.TotalCalories > 0)
            {
                caloriesChangePct = Math.Round(((double)(curCalories - previousWorkoutAgg.TotalCalories) / previousWorkoutAgg.TotalCalories) * 100.0, 1);
            }
            if (previousWorkoutAgg.TotalMinutes > 0)
            {
                minutesChangePct = Math.Round(((double)(curMinutes - previousWorkoutAgg.TotalMinutes) / previousWorkoutAgg.TotalMinutes) * 100.0, 1);
            }
        }

        // 5. Construct response
        var response = new ProgressDashboardResponse
        {
            Period = bounds.Period,
            Range = new DashboardDateRangeDto
            {
                From = bounds.CurrentRange.From,
                To = bounds.CurrentRange.To
            },
            Summary = new DashboardSummaryDto
            {
                Workouts = curWorkouts,
                CaloriesBurned = curCalories,
                Minutes = curMinutes,
                AvgRating = avgRating,
                WeightChange = weightChangeResult.IsSuccess ? weightChangeResult.Value.WeightChange : null
            },
            Comparison = new DashboardComparisonDto
            {
                WorkoutsChangePct = workoutsChangePct,
                CaloriesChangePct = caloriesChangePct,
                MinutesChangePct = minutesChangePct
            },
            Streak = new DashboardStreakDto
            {
                Current = effectiveStreak,
                Longest = longestStreak
            },
            WeightHistory = (weightHistoryResult.IsSuccess && weightHistoryResult.Value != null)
                ? weightHistoryResult.Value.Items.ToList()
                : new List<WeightHistoryDto>(),
            WorkoutHistory = workoutsInRange,
            AchievementsEarned = (achievementsResult.IsSuccess && achievementsResult.Value != null)
                ? achievementsResult.Value
                : new List<UserAchievementDto>()
        };

        // 6. Cache for preset periods
        if (isPreset && cacheKey != null)
        {
            try
            {
                await _cacheService.SetAsync(cacheKey, response, ProgressCacheKeys.DashboardTtl, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache progress dashboard for key {CacheKey}", cacheKey);
            }
        }

        return Result.Success(response);
    }
}
