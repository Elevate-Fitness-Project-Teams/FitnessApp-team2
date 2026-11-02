using Microsoft.Extensions.Logging.Abstractions;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Caching;
using ProgressTrackingService.Common.Pagination;
using ProgressTrackingService.Features.Achievements.DTOs;
using ProgressTrackingService.Features.Progress.Queries.GetProgressDashboard;
using ProgressTrackingService.Features.Progress.Queries.GetWeightHistory;
using ProgressTrackingService.Features.Progress.Queries.ViewUserProgress;
using ProgressTrackingService.Features.Streaks.Queries.GetStreak;
using ProgressTrackingService.Tests.Caching;
using ProgressTrackingService.Tests.Infrastructure;
using Xunit;

namespace ProgressTrackingService.Tests.Progress;

public class GetProgressDashboardQueryHandlerTests
{
    private readonly TestDistributedCache _cache;
    private readonly RedisCacheService _cacheService;

    public GetProgressDashboardQueryHandlerTests()
    {
        _cache = new TestDistributedCache();
        _cacheService = new RedisCacheService(_cache, NullLogger<RedisCacheService>.Instance);
    }

    [Fact]
    public async Task Handle_WhenValidWeekPreset_CalculatesComparisonsAndSummary()
    {
        var userId = Guid.NewGuid();

        var curAgg = new WorkoutAggregateDto(4, 1850, 210, 4.3);
        var prevAgg = new WorkoutAggregateDto(3, 1500, 180, 4.0);
        var weightChange = new WeightChangeDto(80.5, 79.9, -0.6);

        var mediator = new MockMediatorBuilder()
            .RegisterHandler<GetWorkoutAggregateQuery, Result<WorkoutAggregateDto>>(q =>
            {
                // If it's the current period (ends today)
                if (q.To.Date >= DateTime.UtcNow.Date.AddDays(-1))
                    return Task.FromResult(Result.Success(curAgg));
                return Task.FromResult(Result.Success(prevAgg));
            })
            .RegisterHandler<GetWeightChangeInRangeQuery, Result<WeightChangeDto>>(q =>
                Task.FromResult(Result.Success(weightChange)))
            .RegisterHandler<GetWeightHistoryQuery, Result<PagedResult<WeightHistoryDto>>>(q =>
                Task.FromResult(Result.Success(PagedResult<WeightHistoryDto>.Create(new List<WeightHistoryDto>(), 0, 1, 50))))
            .RegisterHandler<GetWorkoutLogsQuery, Result<List<WorkoutLogDto>>>(q =>
                Task.FromResult(Result.Success(new List<WorkoutLogDto>())))
            .RegisterHandler<GetStreakQuery, Result<GetStreakResponse>>(q =>
                Task.FromResult(Result.Success(new GetStreakResponse { CurrentStreak = 5, LongestStreak = 9, LastWorkoutDate = DateTime.UtcNow })))
            .RegisterHandler<GetAchievementsEarnedInRangeQuery, Result<List<UserAchievementDto>>>(q =>
                Task.FromResult(Result.Success(new List<UserAchievementDto>())))
            .Build();

        var handler = new GetProgressDashboardQueryHandler(mediator, _cacheService, NullLogger<GetProgressDashboardQueryHandler>.Instance);

        var result = await handler.Handle(new GetProgressDashboardQuery(userId, "week"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("week", result.Value.Period);
        Assert.Equal(4, result.Value.Summary.Workouts);
        Assert.Equal(1850, result.Value.Summary.CaloriesBurned);
        Assert.Equal(210, result.Value.Summary.Minutes);
        Assert.Equal(4.3, result.Value.Summary.AvgRating);
        Assert.Equal(-0.6, result.Value.Summary.WeightChange);

        // Comparisons:
        // Workouts: (4 - 3) / 3 * 100 = 33.3%
        // Calories: (1850 - 1500) / 1500 * 100 = 23.3%
        // Minutes: (210 - 180) / 180 * 100 = 16.7%
        Assert.Equal(33.3, result.Value.Comparison.WorkoutsChangePct);
        Assert.Equal(23.3, result.Value.Comparison.CaloriesChangePct);
        Assert.Equal(16.7, result.Value.Comparison.MinutesChangePct);

        Assert.Equal(5, result.Value.Streak.Current);
        Assert.Equal(9, result.Value.Streak.Longest);
    }

    [Fact]
    public async Task Handle_WhenPreviousWorkoutsIsZero_ComparisonPctIsNull()
    {
        var userId = Guid.NewGuid();

        var curAgg = new WorkoutAggregateDto(2, 600, 60, 5.0);
        var prevAgg = new WorkoutAggregateDto(0, 0, 0, 0.0); // 0 workouts in previous period
        var weightChange = new WeightChangeDto(null, null, null);

        var mediator = new MockMediatorBuilder()
            .RegisterHandler<GetWorkoutAggregateQuery, Result<WorkoutAggregateDto>>(q =>
            {
                if (q.To.Date >= DateTime.UtcNow.Date.AddDays(-1))
                    return Task.FromResult(Result.Success(curAgg));
                return Task.FromResult(Result.Success(prevAgg));
            })
            .RegisterHandler<GetWeightChangeInRangeQuery, Result<WeightChangeDto>>(q =>
                Task.FromResult(Result.Success(weightChange)))
            .RegisterHandler<GetWeightHistoryQuery, Result<PagedResult<WeightHistoryDto>>>(q =>
                Task.FromResult(Result.Success(PagedResult<WeightHistoryDto>.Create(new List<WeightHistoryDto>(), 0, 1, 50))))
            .RegisterHandler<GetWorkoutLogsQuery, Result<List<WorkoutLogDto>>>(q =>
                Task.FromResult(Result.Success(new List<WorkoutLogDto>())))
            .RegisterHandler<GetStreakQuery, Result<GetStreakResponse>>(q =>
                Task.FromResult(Result.Success(new GetStreakResponse())))
            .RegisterHandler<GetAchievementsEarnedInRangeQuery, Result<List<UserAchievementDto>>>(q =>
                Task.FromResult(Result.Success(new List<UserAchievementDto>())))
            .Build();

        var handler = new GetProgressDashboardQueryHandler(mediator, _cacheService, NullLogger<GetProgressDashboardQueryHandler>.Instance);

        var result = await handler.Handle(new GetProgressDashboardQuery(userId, "week"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.Comparison.WorkoutsChangePct);
        Assert.Null(result.Value.Comparison.CaloriesChangePct);
        Assert.Null(result.Value.Comparison.MinutesChangePct);
    }

    [Fact]
    public async Task Handle_CachesPresetPeriod_ServesSubsequentCallsFromCache()
    {
        var userId = Guid.NewGuid();
        int aggregateCalls = 0;

        var mediator = new MockMediatorBuilder()
            .RegisterHandler<GetWorkoutAggregateQuery, Result<WorkoutAggregateDto>>(q =>
            {
                aggregateCalls++;
                return Task.FromResult(Result.Success(new WorkoutAggregateDto(1, 100, 30, 5.0)));
            })
            .RegisterHandler<GetWeightChangeInRangeQuery, Result<WeightChangeDto>>(q =>
                Task.FromResult(Result.Success(new WeightChangeDto(null, null, null))))
            .RegisterHandler<GetWeightHistoryQuery, Result<PagedResult<WeightHistoryDto>>>(q =>
                Task.FromResult(Result.Success(PagedResult<WeightHistoryDto>.Create(new List<WeightHistoryDto>(), 0, 1, 50))))
            .RegisterHandler<GetWorkoutLogsQuery, Result<List<WorkoutLogDto>>>(q =>
                Task.FromResult(Result.Success(new List<WorkoutLogDto>())))
            .RegisterHandler<GetStreakQuery, Result<GetStreakResponse>>(q =>
                Task.FromResult(Result.Success(new GetStreakResponse())))
            .RegisterHandler<GetAchievementsEarnedInRangeQuery, Result<List<UserAchievementDto>>>(q =>
                Task.FromResult(Result.Success(new List<UserAchievementDto>())))
            .Build();

        var handler = new GetProgressDashboardQueryHandler(mediator, _cacheService, NullLogger<GetProgressDashboardQueryHandler>.Instance);

        // First call caches response
        var res1 = await handler.Handle(new GetProgressDashboardQuery(userId, "month"), CancellationToken.None);
        Assert.True(res1.IsSuccess);
        Assert.True(aggregateCalls > 0);

        var recordedCalls = aggregateCalls;

        // Second call should serve from cache
        var res2 = await handler.Handle(new GetProgressDashboardQuery(userId, "month"), CancellationToken.None);
        Assert.True(res2.IsSuccess);
        Assert.Equal(recordedCalls, aggregateCalls); // No further queries made
    }
}
