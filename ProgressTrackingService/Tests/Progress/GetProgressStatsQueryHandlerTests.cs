using Microsoft.Extensions.Logging.Abstractions;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Caching;
using ProgressTrackingService.Common.Errors;
using ProgressTrackingService.Features.Progress.Queries.GetProgressStats;
using ProgressTrackingService.Features.Progress.Queries.ViewUserProgress;
using ProgressTrackingService.Features.Streaks.Queries.GetStreak;
using ProgressTrackingService.Tests.Caching;
using ProgressTrackingService.Tests.Infrastructure;
using Xunit;

namespace ProgressTrackingService.Tests.Progress;

public class GetProgressStatsQueryHandlerTests
{
    private readonly TestDistributedCache _cache;
    private readonly RedisCacheService _cacheService;

    public GetProgressStatsQueryHandlerTests()
    {
        _cache = new TestDistributedCache();
        _cacheService = new RedisCacheService(_cache, NullLogger<RedisCacheService>.Instance);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();

        var mediator = new MockMediatorBuilder()
            .RegisterHandler<CheckUserExistsQuery, Result<bool>>(q =>
                Task.FromResult(Result.Success(false)))
            .Build();

        var handler = new GetProgressStatsQueryHandler(
            mediator,
            _cacheService,
            NullLogger<GetProgressStatsQueryHandler>.Instance);

        var result = await handler.Handle(new GetProgressStatsQuery(userId), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ProgressErrors.ResUserNotFound, result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenUserExistsWithActiveStreak_ReturnsMergedStats()
    {
        var userId = Guid.NewGuid();

        var stats = new UserStatisticDto
        {
            TotalWorkouts = 12,
            TotalCaloriesBurned = 3500,
            TotalMinutes = 480,
            CurrentWeight = 77.5,
            TotalWeightLost = 4.2,
            UpdatedAt = DateTime.UtcNow
        };

        var streak = new GetStreakResponse
        {
            UserId = userId,
            CurrentStreak = 5,
            LongestStreak = 8,
            LastWorkoutDate = DateTime.UtcNow.Date
        };

        var mediator = new MockMediatorBuilder()
            .RegisterHandler<CheckUserExistsQuery, Result<bool>>(q =>
                Task.FromResult(Result.Success(true)))
            .RegisterHandler<GetUserStatisticsQuery, Result<UserStatisticDto?>>(q =>
                Task.FromResult(Result.Success<UserStatisticDto?>(stats)))
            .RegisterHandler<GetStreakQuery, Result<GetStreakResponse>>(q =>
                Task.FromResult(Result.Success(streak)))
            .Build();

        var handler = new GetProgressStatsQueryHandler(
            mediator,
            _cacheService,
            NullLogger<GetProgressStatsQueryHandler>.Instance);

        var result = await handler.Handle(new GetProgressStatsQuery(userId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(12, result.Value.TotalWorkouts);
        Assert.Equal(3500, result.Value.TotalCaloriesBurned);
        Assert.Equal(480, result.Value.TotalMinutes);
        Assert.Equal(77.5, result.Value.CurrentWeight);
        Assert.Equal(4.2, result.Value.TotalWeightLost);
        Assert.Equal(5, result.Value.CurrentStreak);
        Assert.Equal(8, result.Value.LongestStreak);
    }

    [Fact]
    public async Task Handle_WhenStreakLapsed_ReportsCurrentStreakAsZero()
    {
        var userId = Guid.NewGuid();

        var lapsedStreak = new GetStreakResponse
        {
            UserId = userId,
            CurrentStreak = 10,
            LongestStreak = 14,
            LastWorkoutDate = DateTime.UtcNow.AddDays(-3) // 3 days ago
        };

        var mediator = new MockMediatorBuilder()
            .RegisterHandler<CheckUserExistsQuery, Result<bool>>(q =>
                Task.FromResult(Result.Success(true)))
            .RegisterHandler<GetUserStatisticsQuery, Result<UserStatisticDto?>>(q =>
                Task.FromResult(Result.Success<UserStatisticDto?>(new UserStatisticDto())))
            .RegisterHandler<GetStreakQuery, Result<GetStreakResponse>>(q =>
                Task.FromResult(Result.Success(lapsedStreak)))
            .Build();

        var handler = new GetProgressStatsQueryHandler(
            mediator,
            _cacheService,
            NullLogger<GetProgressStatsQueryHandler>.Instance);

        var result = await handler.Handle(new GetProgressStatsQuery(userId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.CurrentStreak); // Decayed to 0
        Assert.Equal(14, result.Value.LongestStreak); // Kept longest
    }

    [Fact]
    public async Task Handle_CachesResult_ServesSubsequentCallsFromCache()
    {
        var userId = Guid.NewGuid();
        int dbCallCount = 0;

        var mediator = new MockMediatorBuilder()
            .RegisterHandler<CheckUserExistsQuery, Result<bool>>(q =>
            {
                dbCallCount++;
                return Task.FromResult(Result.Success(true));
            })
            .RegisterHandler<GetUserStatisticsQuery, Result<UserStatisticDto?>>(q =>
                Task.FromResult(Result.Success<UserStatisticDto?>(new UserStatisticDto { TotalWorkouts = 1 })))
            .RegisterHandler<GetStreakQuery, Result<GetStreakResponse>>(q =>
                Task.FromResult(Result.Success(new GetStreakResponse { CurrentStreak = 1, LongestStreak = 1, LastWorkoutDate = DateTime.UtcNow })))
            .Build();

        var handler = new GetProgressStatsQueryHandler(
            mediator,
            _cacheService,
            NullLogger<GetProgressStatsQueryHandler>.Instance);

        // First call populates cache
        var result1 = await handler.Handle(new GetProgressStatsQuery(userId), CancellationToken.None);
        Assert.True(result1.IsSuccess);
        Assert.Equal(1, dbCallCount);

        // Second call should hit cache and NOT invoke CheckUserExistsQuery
        var result2 = await handler.Handle(new GetProgressStatsQuery(userId), CancellationToken.None);
        Assert.True(result2.IsSuccess);
        Assert.Equal(1, result2.Value.TotalWorkouts);
        Assert.Equal(1, dbCallCount); // Assert no additional query executions
    }
}
