using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Caching;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Common.Domain;
using ProgressTrackingService.Common.Errors;
using ProgressTrackingService.Common.Grpc;
using ProgressTrackingService.Features.Achievements.DTOs;
using ProgressTrackingService.Features.Achievements.Orchestrators.EvaluateAchievements;
using ProgressTrackingService.Features.Streaks.Commands.UpdateStreak;
using ProgressTrackingService.Features.WorkoutLogs.Commands.SaveWorkoutLog;
using ProgressTrackingService.Features.WorkoutLogs.Commands.UpdateStatistic;
using ProgressTrackingService.Features.WorkoutLogs.DTOs;
using ProgressTrackingService.Features.WorkoutLogs.Orchestrators.LogWorkout;
using ProgressTrackingService.Features.WorkoutLogs.Queries.GetWorkoutLogBySession;
using ProgressTrackingService.Models;
using ProgressTrackingService.Tests.Caching;
using ProgressTrackingService.Tests.Infrastructure;
using Xunit;

namespace ProgressTrackingService.Tests.WorkoutLogs;

public class TestWorkoutSessionClient : IWorkoutSessionClient
{
    public bool ShouldSucceed { get; set; } = true;
    public Error FailureError { get; set; } = Error.NotFound(ProgressErrors.ResSessionNotFound, "Session not found.");
    public int CallCount { get; private set; }

    public Task<Result> CompleteSessionAsync(string sessionId, Guid userId, CancellationToken cancellationToken = default)
    {
        CallCount++;
        return Task.FromResult(ShouldSucceed ? Result.Success() : Result.Failure(FailureError));
    }
}

public class LogWorkoutOrchestratorTests : IDisposable
{
    private readonly SqliteTestDbContext _db;
    private readonly TestDistributedCache _testCache;
    private readonly RedisCacheService _cacheService;
    private readonly CacheInvalidator _invalidator;
    private readonly TestPublishEndpoint _publishEndpoint;
    private readonly TestWorkoutSessionClient _sessionClient;

    public LogWorkoutOrchestratorTests()
    {
        _db = new SqliteTestDbContext();
        _testCache = new TestDistributedCache();
        _cacheService = new RedisCacheService(_testCache, NullLogger<RedisCacheService>.Instance);
        _invalidator = new CacheInvalidator(_cacheService, NullLogger<CacheInvalidator>.Instance);
        _publishEndpoint = new TestPublishEndpoint();
        _sessionClient = new TestWorkoutSessionClient();
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    [Fact]
    public async Task Handle_WhenSessionAlreadyLoggedLocally_ReturnsExistingLogIdIdempotentlyWithoutCallingSessionClient()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var workoutId = Guid.NewGuid();
        var existingLogId = Guid.NewGuid();
        var sessionId = "session-123";

        _db.Context.WorkoutLogs.Add(new WorkoutLog
        {
            Id = existingLogId,
            UserId = userId,
            WorkoutId = workoutId,
            SessionId = sessionId,
            CompletedAt = DateTime.UtcNow.AddMinutes(-10),
            DurationInMinutes = 30,
            CaloriesBurned = 250,
            Difficulty = WorkoutDifficulty.Beginner,
            Rating = 5
        });
        await _db.Context.SaveChangesAsync();

        var mediatorMock = new MockMediatorBuilder()
            .RegisterHandler<GetWorkoutLogBySessionQuery, Result<Guid?>>(q =>
                Task.FromResult(Result.Success<Guid?>(existingLogId)))
            .Build();

        var uow = new UnitOfWork(_db.Context);
        var handler = new LogWorkoutOrchestratorHandler(
            mediatorMock,
            _sessionClient,
            uow,
            _invalidator,
            _publishEndpoint,
            NullLogger<LogWorkoutOrchestratorHandler>.Instance);

        var request = new LogWorkoutOrchestrator(
            userId,
            workoutId,
            sessionId,
            DateTime.UtcNow,
            30,
            250,
            "Beginner",
            null,
            5,
            new List<LogWorkoutExerciseDto>());

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(existingLogId, result.Value.LogId);
        Assert.False(result.Value.StreakUpdated);
        Assert.Equal(0, _sessionClient.CallCount);
    }

    [Fact]
    public async Task Handle_WhenSessionClientFails_ReturnsFailureWithoutSideEffects()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var workoutId = Guid.NewGuid();
        var sessionId = "session-404";

        _sessionClient.ShouldSucceed = false;

        var mediatorMock = new MockMediatorBuilder()
            .RegisterHandler<GetWorkoutLogBySessionQuery, Result<Guid?>>(q =>
                Task.FromResult(Result.Success<Guid?>(null)))
            .Build();

        var uow = new UnitOfWork(_db.Context);
        var handler = new LogWorkoutOrchestratorHandler(
            mediatorMock,
            _sessionClient,
            uow,
            _invalidator,
            _publishEndpoint,
            NullLogger<LogWorkoutOrchestratorHandler>.Instance);

        var request = new LogWorkoutOrchestrator(
            userId,
            workoutId,
            sessionId,
            DateTime.UtcNow,
            30,
            250,
            "Beginner",
            null,
            5,
            new List<LogWorkoutExerciseDto>());

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ProgressErrors.ResSessionNotFound, result.Error.Code);
        Assert.Empty(_publishEndpoint.PublishedMessages);
    }

    [Fact]
    public async Task Handle_WhenSuccessful_ExecutesInsideUnitOfWorkAndPublishesOutboxEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var workoutId = Guid.NewGuid();
        var logId = Guid.NewGuid();
        var sessionId = "session-success";

        var mediatorMock = new MockMediatorBuilder()
            .RegisterHandler<GetWorkoutLogBySessionQuery, Result<Guid?>>(q =>
                Task.FromResult(Result.Success<Guid?>(null)))
            .RegisterHandler<SaveWorkoutLogCommand, Result<Guid>>(cmd =>
                Task.FromResult(Result.Success(logId)))
            .RegisterHandler<UpdateStatisticCommand, Result>(cmd =>
                Task.FromResult(Result.Success()))
            .RegisterHandler<UpdateStreakCommand, Result<StreakCalculationResult>>(cmd =>
                Task.FromResult(Result.Success(new StreakCalculationResult(CurrentStreak: 3, LongestStreak: 5, StreakUpdated: true))))
            .RegisterHandler<EvaluateAchievementsOrchestrator, Result<List<EarnedAchievementDto>>>(cmd =>
                Task.FromResult(Result.Success(new List<EarnedAchievementDto>())))
            .Build();

        var uow = new UnitOfWork(_db.Context);
        var handler = new LogWorkoutOrchestratorHandler(
            mediatorMock,
            _sessionClient,
            uow,
            _invalidator,
            _publishEndpoint,
            NullLogger<LogWorkoutOrchestratorHandler>.Instance);

        var request = new LogWorkoutOrchestrator(
            userId,
            workoutId,
            sessionId,
            DateTime.UtcNow,
            45,
            350,
            "Intermediate",
            "Great workout",
            5,
            new List<LogWorkoutExerciseDto>());

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(logId, result.Value.LogId);
        Assert.True(result.Value.StreakUpdated);
        Assert.Equal(3, result.Value.CurrentStreak);
        Assert.Single(_publishEndpoint.PublishedMessages);
    }

    [Fact]
    public async Task Handle_WhenInnerCommandFails_RollsBackTransactionAndReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var workoutId = Guid.NewGuid();
        var sessionId = "session-fail";

        var mediatorMock = new MockMediatorBuilder()
            .RegisterHandler<GetWorkoutLogBySessionQuery, Result<Guid?>>(q =>
                Task.FromResult(Result.Success<Guid?>(null)))
            .RegisterHandler<SaveWorkoutLogCommand, Result<Guid>>(cmd =>
                Task.FromResult(Result.Failure<Guid>(Error.Failure("DB.SaveFailed", "Disk write failure."))))
            .Build();

        var uow = new UnitOfWork(_db.Context);
        var handler = new LogWorkoutOrchestratorHandler(
            mediatorMock,
            _sessionClient,
            uow,
            _invalidator,
            _publishEndpoint,
            NullLogger<LogWorkoutOrchestratorHandler>.Instance);

        var request = new LogWorkoutOrchestrator(
            userId,
            workoutId,
            sessionId,
            DateTime.UtcNow,
            30,
            200,
            "Beginner",
            null,
            4,
            new List<LogWorkoutExerciseDto>());

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Disk write failure", result.Error.Description);
        Assert.Empty(_publishEndpoint.PublishedMessages);
    }
}


