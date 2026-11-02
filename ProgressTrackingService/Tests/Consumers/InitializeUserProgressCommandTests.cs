using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Features.Users.Commands.InitializeUserProgress;
using ProgressTrackingService.Models;
using ProgressTrackingService.Tests.Infrastructure;
using Xunit;

namespace ProgressTrackingService.Tests.Consumers;

public class InitializeUserProgressCommandTests : IDisposable
{
    private readonly SqliteTestDbContext _db;
    private readonly GeneralRepo<Streak> _streakRepo;
    private readonly GeneralRepo<UserStatistic> _userStatsRepo;
    private readonly UnitOfWork _unitOfWork;

    public InitializeUserProgressCommandTests()
    {
        _db = new SqliteTestDbContext();
        _streakRepo = new GeneralRepo<Streak>(_db.Context);
        _userStatsRepo = new GeneralRepo<UserStatistic>(_db.Context);
        _unitOfWork = new UnitOfWork(_db.Context);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    [Fact]
    public async Task Handle_WhenNewUser_InitializesBothStreakAndUserStatistic()
    {
        var userId = Guid.NewGuid();
        var handler = new InitializeUserProgressCommandHandler(_streakRepo, _userStatsRepo, _unitOfWork);

        var result = await handler.Handle(new InitializeUserProgressCommand(userId), CancellationToken.None);

        Assert.True(result.IsSuccess);

        var streak = await _db.Context.Streaks.FirstOrDefaultAsync(s => s.UserId == userId);
        var stats = await _db.Context.UserStatistics.FirstOrDefaultAsync(s => s.UserId == userId);

        Assert.NotNull(streak);
        Assert.Equal(0, streak.CurrentStreak);
        Assert.Equal(0, streak.LongestStreak);
        Assert.Null(streak.LastWorkoutDate);

        Assert.NotNull(stats);
        Assert.Equal(0, stats.TotalWorkouts);
        Assert.Equal(0, stats.TotalCaloriesBurned);
        Assert.Equal(0, stats.TotalMinutes);
        Assert.Equal(0.0, stats.TotalWeightLost);
    }

    [Fact]
    public async Task Handle_WhenCalledMultipleTimes_IsIdempotentAndDoesNotDuplicate()
    {
        var userId = Guid.NewGuid();
        var handler = new InitializeUserProgressCommandHandler(_streakRepo, _userStatsRepo, _unitOfWork);

        // First execution
        var result1 = await handler.Handle(new InitializeUserProgressCommand(userId), CancellationToken.None);
        Assert.True(result1.IsSuccess);

        // Second execution (simulating duplicate message delivery)
        var result2 = await handler.Handle(new InitializeUserProgressCommand(userId), CancellationToken.None);
        Assert.True(result2.IsSuccess);

        var streakCount = await _db.Context.Streaks.CountAsync(s => s.UserId == userId);
        var statsCount = await _db.Context.UserStatistics.CountAsync(s => s.UserId == userId);

        Assert.Equal(1, streakCount);
        Assert.Equal(1, statsCount);
    }
}
