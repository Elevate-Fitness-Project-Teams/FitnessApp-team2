using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Features.Progress.Commands.LogWeight;
using ProgressTrackingService.Features.Progress.Queries.GetWeightBeforeDate;
using ProgressTrackingService.Features.Progress.Queries.GetWeightBounds;
using ProgressTrackingService.Models;
using ProgressTrackingService.Tests.Infrastructure;
using Xunit;

namespace ProgressTrackingService.Tests.Weight;

public class WeightTests : IDisposable
{
    private readonly SqliteTestDbContext _db;
    private readonly GeneralRepo<WeightHistory> _weightRepo;
    private readonly GeneralRepo<UserStatistic> _statsRepo;
    private readonly UnitOfWork _uow;

    public WeightTests()
    {
        _db = new SqliteTestDbContext();
        _weightRepo = new GeneralRepo<WeightHistory>(_db.Context);
        _statsRepo = new GeneralRepo<UserStatistic>(_db.Context);
        _uow = new UnitOfWork(_db.Context);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    [Fact]
    public async Task GetWeightBeforeDate_ReturnsMostRecentEntryBeforeGivenDate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var baseDate = new DateTime(2026, 5, 10, 10, 0, 0, DateTimeKind.Utc);

        _db.Context.WeightHistories.AddRange(
            new WeightHistory { Id = Guid.NewGuid(), UserId = userId, Weight = 85.0, Date = baseDate.AddDays(-5) },
            new WeightHistory { Id = Guid.NewGuid(), UserId = userId, Weight = 84.0, Date = baseDate.AddDays(-1) },
            new WeightHistory { Id = Guid.NewGuid(), UserId = userId, Weight = 82.0, Date = baseDate.AddDays(2) } // After
        );
        await _db.Context.SaveChangesAsync();

        var handler = new GetWeightBeforeDateHandler(_weightRepo);

        // Act
        var result = await handler.Handle(new GetWeightBeforeDateQuery(userId, baseDate), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(84.0, result.Value);
    }

    [Fact]
    public async Task GetWeightBounds_CalculatesInitialAndLatestCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var t1 = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var t2 = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);

        _db.Context.WeightHistories.AddRange(
            new WeightHistory { Id = Guid.NewGuid(), UserId = userId, Weight = 90.0, Date = t1 },
            new WeightHistory { Id = Guid.NewGuid(), UserId = userId, Weight = 85.0, Date = t2 }
        );
        await _db.Context.SaveChangesAsync();

        var handler = new GetWeightBoundsHandler(_weightRepo);

        // Act 1: Logging a newest date
        var result1 = await handler.Handle(new GetWeightBoundsQuery(userId, t2.AddDays(1)), CancellationToken.None);
        Assert.True(result1.IsSuccess);
        Assert.Equal(90.0, result1.Value.InitialWeight);
        Assert.Equal(85.0, result1.Value.LatestWeight);
        Assert.True(result1.Value.IsNewEntryLatest);

        // Act 2: Logging a back-dated entry
        var result2 = await handler.Handle(new GetWeightBoundsQuery(userId, t1.AddDays(5)), CancellationToken.None);
        Assert.True(result2.IsSuccess);
        Assert.False(result2.Value.IsNewEntryLatest);
    }

    [Fact]
    public async Task UpdateWeightStatistic_CalculatesTotalWeightLostCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var handler = new UpdateWeightStatisticHandler(_statsRepo, _uow);

        // Act
        var result = await handler.Handle(new UpdateWeightStatisticCommand(userId, 95.0, 88.5), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var stats = await _db.Context.UserStatistics.FirstOrDefaultAsync(s => s.UserId == userId);
        Assert.NotNull(stats);
        Assert.Equal(95.0, stats.InitialWeight);
        Assert.Equal(88.5, stats.CurrentWeight);
        Assert.Equal(6.5, stats.TotalWeightLost);
    }
}
