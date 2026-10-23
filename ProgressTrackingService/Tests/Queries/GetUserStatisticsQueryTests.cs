using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Common.Errors;
using ProgressTrackingService.Features.Progress.Queries.ViewUserProgress;
using ProgressTrackingService.Models;
using ProgressTrackingService.Tests.Infrastructure;
using Xunit;

namespace ProgressTrackingService.Tests.Queries;

public class GetUserStatisticsQueryTests : IDisposable
{
    private readonly SqliteTestDbContext _db;
    private readonly IGeneralRepo<UserStatistic> _repo;

    public GetUserStatisticsQueryTests()
    {
        _db = new SqliteTestDbContext();
        _repo = new GeneralRepo<UserStatistic>(_db.Context);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    [Fact]
    public async Task Handle_WhenUserHasNoStatistics_ReturnsNotFoundWithResUserNotFound()
    {
        // Arrange
        var handler = new GetUserStatisticsQueryHandler(_repo);
        var query = new GetUserStatisticsQuery(Guid.NewGuid());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ProgressErrors.ResUserNotFound, result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenUserStatisticsExist_ReturnsSuccessWithMappedDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var stat = new UserStatistic
        {
            UserId = userId,
            TotalWorkouts = 12,
            TotalCaloriesBurned = 4500,
            TotalMinutes = 360,
            InitialWeight = 85.0,
            CurrentWeight = 82.5,
            TotalWeightLost = 2.5,
            UpdatedAt = DateTime.UtcNow
        };
        await _repo.AddAsync(stat);
        await _db.Context.SaveChangesAsync();

        var handler = new GetUserStatisticsQueryHandler(_repo);
        var query = new GetUserStatisticsQuery(userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(12, result.Value.TotalWorkouts);
        Assert.Equal(4500, result.Value.TotalCaloriesBurned);
        Assert.Equal(2.5, result.Value.TotalWeightLost);
    }
}
