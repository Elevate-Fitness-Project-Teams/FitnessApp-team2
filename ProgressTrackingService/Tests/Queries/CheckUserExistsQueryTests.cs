using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Features.Progress.Queries.ViewUserProgress;
using ProgressTrackingService.Models;
using ProgressTrackingService.Tests.Infrastructure;
using Xunit;

namespace ProgressTrackingService.Tests.Queries;

public class CheckUserExistsQueryTests : IDisposable
{
    private readonly SqliteTestDbContext _db;
    private readonly CheckUserExistsQueryHandler _handler;

    public CheckUserExistsQueryTests()
    {
        _db = new SqliteTestDbContext();
        var streakRepo = new GeneralRepo<Streak>(_db.Context);
        var statsRepo = new GeneralRepo<UserStatistic>(_db.Context);
        var weightRepo = new GeneralRepo<WeightHistory>(_db.Context);
        var workoutRepo = new GeneralRepo<WorkoutLog>(_db.Context);

        _handler = new CheckUserExistsQueryHandler(streakRepo, statsRepo, weightRepo, workoutRepo);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    [Fact]
    public async Task Handle_WhenUserHasNoRecords_ReturnsFalse()
    {
        var result = await _handler.Handle(new CheckUserExistsQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value);
    }

    [Fact]
    public async Task Handle_WhenUserHasStreakRecord_ReturnsTrue()
    {
        var userId = Guid.NewGuid();
        _db.Context.Streaks.Add(new Streak { UserId = userId, CurrentStreak = 1, LongestStreak = 1 });
        await _db.Context.SaveChangesAsync();

        var result = await _handler.Handle(new CheckUserExistsQuery(userId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task Handle_WhenUserHasWeightRecord_ReturnsTrue()
    {
        var userId = Guid.NewGuid();
        _db.Context.WeightHistories.Add(new WeightHistory { Id = Guid.NewGuid(), UserId = userId, Weight = 75, Date = DateTime.UtcNow });
        await _db.Context.SaveChangesAsync();

        var result = await _handler.Handle(new CheckUserExistsQuery(userId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }
}
