using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using MessageBroker.Events;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Caching;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Common.Errors;
using ProgressTrackingService.Features.Achievements.DTOs;
using ProgressTrackingService.Features.Achievements.Orchestrators.EvaluateAchievements;
using ProgressTrackingService.Features.Biometrics.Queries.GetUserHeight;
using ProgressTrackingService.Features.Progress.Commands.LogWeight;
using ProgressTrackingService.Features.Progress.Orchestrators;
using ProgressTrackingService.Features.Progress.Queries.GetWeightBeforeDate;
using ProgressTrackingService.Features.Progress.Queries.GetWeightBounds;
using ProgressTrackingService.Tests.Caching;
using ProgressTrackingService.Tests.Infrastructure;
using Xunit;

namespace ProgressTrackingService.Tests.Progress;

public class LogWeightOrchestratorTests : IDisposable
{
    private readonly SqliteTestDbContext _db;
    private readonly TestDistributedCache _testCache;
    private readonly RedisCacheService _cacheService;
    private readonly CacheInvalidator _invalidator;
    private readonly TestPublishEndpoint _publishEndpoint;

    public LogWeightOrchestratorTests()
    {
        _db = new SqliteTestDbContext();
        _testCache = new TestDistributedCache();
        _cacheService = new RedisCacheService(_testCache, NullLogger<RedisCacheService>.Instance);
        _invalidator = new CacheInvalidator(_cacheService, NullLogger<CacheInvalidator>.Instance);
        _publishEndpoint = new TestPublishEndpoint();
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    [Fact]
    public async Task Handle_WhenSuccessful_ExecutesInsideUnitOfWorkAndPublishesWeightUpdatedEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var weightId = Guid.NewGuid();
        var date = DateTime.UtcNow;

        var mediatorMock = new MockMediatorBuilder()
            .RegisterHandler<GetWeightBeforeDateQuery, Result<double?>>(q =>
                Task.FromResult(Result.Success<double?>(82.0)))
            .RegisterHandler<SaveWeightEntryCommand, Result<Guid>>(cmd =>
                Task.FromResult(Result.Success(weightId)))
            .RegisterHandler<GetWeightBoundsQuery, Result<WeightBoundsDto>>(q =>
                Task.FromResult(Result.Success(new WeightBoundsDto(85.0, 80.0, true))))
            .RegisterHandler<UpdateWeightStatisticCommand, Result>(cmd =>
                Task.FromResult(Result.Success()))
            .RegisterHandler<EvaluateAchievementsOrchestrator, Result<List<EarnedAchievementDto>>>(cmd =>
                Task.FromResult(Result.Success(new List<EarnedAchievementDto>())))
            .RegisterHandler<GetUserHeightQuery, Result<double?>>(q =>
                Task.FromResult(Result.Success<double?>(180.0)))
            .Build();

        var uow = new UnitOfWork(_db.Context);
        var handler = new LogWeightOrchestratorHandler(
            mediatorMock,
            uow,
            _invalidator,
            _publishEndpoint,
            NullLogger<LogWeightOrchestratorHandler>.Instance);

        var request = new LogWeightOrchestrator(userId, 80.0, date, "Morning weigh-in");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(weightId, result.Value.Id);
        Assert.Equal(80.0, result.Value.Weight);
        Assert.Equal(-2.0, result.Value.DifferenceFromPrevious);
        Assert.Equal(5.0, result.Value.TotalWeightLost);
        Assert.NotNull(result.Value.Bmi);
        Assert.Single(_publishEndpoint.PublishedMessages);
        var evt = Assert.IsType<WeightUpdatedEvent>(_publishEndpoint.PublishedMessages[0]);
        Assert.Equal(80.0, evt.NewWeight);
        Assert.Equal(userId, evt.UserId);
    }

    [Fact]
    public async Task Handle_WhenSaveCommandFails_RollsBackTransactionAndReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = DateTime.UtcNow;

        var mediatorMock = new MockMediatorBuilder()
            .RegisterHandler<GetWeightBeforeDateQuery, Result<double?>>(q =>
                Task.FromResult(Result.Success<double?>(null)))
            .RegisterHandler<SaveWeightEntryCommand, Result<Guid>>(cmd =>
                Task.FromResult(Result.Failure<Guid>(Error.Failure("DB.Error", "Database constraint violated."))))
            .Build();

        var uow = new UnitOfWork(_db.Context);
        var handler = new LogWeightOrchestratorHandler(
            mediatorMock,
            uow,
            _invalidator,
            _publishEndpoint,
            NullLogger<LogWeightOrchestratorHandler>.Instance);

        var request = new LogWeightOrchestrator(userId, 75.0, date, null);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Database constraint violated", result.Error.Description);
        Assert.Empty(_publishEndpoint.PublishedMessages);
    }
}
