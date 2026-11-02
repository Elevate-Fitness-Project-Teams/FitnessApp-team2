using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Errors;
using ProgressTrackingService.Common.Pagination;
using ProgressTrackingService.Features.Progress.Queries.GetProgressStats;
using ProgressTrackingService.Features.Progress.Queries.GetWeightHistory;
using ProgressTrackingService.Features.Progress.Queries.ViewUserProgress;
using ProgressTrackingService.Tests.Infrastructure;
using Xunit;

namespace ProgressTrackingService.Tests.Progress;

public class ViewUserProgressOrchestratorTests
{
    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();

        var mediator = new MockMediatorBuilder()
            .RegisterHandler<CheckUserExistsQuery, Result<bool>>(q =>
                Task.FromResult(Result.Success(false)))
            .Build();

        var handler = new ViewUserProgressOrchestratorHandler(mediator);
        var result = await handler.Handle(new ViewUserProgressOrchestrator(userId), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ProgressErrors.ResUserNotFound, result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ComposesStatsAndBoundedLists()
    {
        var userId = Guid.NewGuid();

        var stats = new ProgressStatsDto
        {
            TotalWorkouts = 15,
            TotalCaloriesBurned = 4500,
            TotalMinutes = 600,
            TotalWeightLost = 5.0,
            CurrentWeight = 75.0,
            CurrentStreak = 3,
            LongestStreak = 7
        };

        var workoutLogs = new List<WorkoutLogDto>
        {
            new() { Id = Guid.NewGuid(), WorkoutId = Guid.NewGuid(), CompletedAt = DateTime.UtcNow, DurationInMinutes = 45 }
        };

        var weightEntries = new List<WeightHistoryDto>
        {
            new() { Id = Guid.NewGuid(), Weight = 75.0, Date = DateTime.UtcNow }
        };
        var pagedWeights = PagedResult<WeightHistoryDto>.Create(weightEntries, 1, 1, 30);

        var mediator = new MockMediatorBuilder()
            .RegisterHandler<CheckUserExistsQuery, Result<bool>>(q =>
                Task.FromResult(Result.Success(true)))
            .RegisterHandler<GetProgressStatsQuery, Result<ProgressStatsDto>>(q =>
                Task.FromResult(Result.Success(stats)))
            .RegisterHandler<GetWorkoutLogsQuery, Result<List<WorkoutLogDto>>>(q =>
            {
                Assert.Equal(10, q.Limit); // Assert bounded limit
                return Task.FromResult(Result.Success(workoutLogs));
            })
            .RegisterHandler<GetWeightHistoryQuery, Result<PagedResult<WeightHistoryDto>>>(q =>
            {
                Assert.Equal(30, q.PageSize); // Assert bounded page size
                return Task.FromResult(Result.Success(pagedWeights));
            })
            .Build();

        var handler = new ViewUserProgressOrchestratorHandler(mediator);
        var result = await handler.Handle(new ViewUserProgressOrchestrator(userId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.Statistics);
        Assert.Equal(15, result.Value.Statistics.TotalWorkouts);
        Assert.Equal(3, result.Value.Statistics.CurrentStreak);
        Assert.Single(result.Value.WorkoutLogs);
        Assert.Single(result.Value.WeightHistory);
    }
}
