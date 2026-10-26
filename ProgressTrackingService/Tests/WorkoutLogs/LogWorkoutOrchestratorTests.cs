using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Caching;
using ProgressTrackingService.Common.Database;
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

        var handler = new LogWorkoutOrchestratorHandler(
            mediatorMock,
            _sessionClient,
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

        var handler = new LogWorkoutOrchestratorHandler(
            mediatorMock,
            _sessionClient,
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
}

// Lightweight test dispatcher for orchestrator integration unit testing
public class MockMediatorBuilder
{
    private readonly Dictionary<Type, Func<object, Task<object>>> _handlers = new();

    public MockMediatorBuilder RegisterHandler<TRequest, TResponse>(Func<TRequest, Task<TResponse>> handler)
    {
        _handlers[typeof(TRequest)] = async req =>
        {
            var res = await handler((TRequest)req);
            return res!;
        };
        return this;
    }

    public IMediator Build() => new MockMediator(_handlers);

    private class MockMediator : IMediator
    {
        private readonly Dictionary<Type, Func<object, Task<object>>> _handlers;

        public MockMediator(Dictionary<Type, Func<object, Task<object>>> handlers)
        {
            _handlers = handlers;
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var reqType = request.GetType();
            if (_handlers.TryGetValue(reqType, out var handler))
            {
                var result = await handler(request);
                return (TResponse)result;
            }
            throw new NotImplementedException($"No mock registered for {reqType.Name}");
        }

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest =>
            Task.CompletedTask;

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task Publish(object notification, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification =>
            Task.CompletedTask;

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }
}
