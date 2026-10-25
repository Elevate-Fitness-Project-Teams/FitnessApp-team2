using Microsoft.Extensions.Logging.Abstractions;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Features.Biometrics.Commands.UpsertUserBiometrics;
using ProgressTrackingService.Features.Biometrics.Queries.GetUserHeight;
using ProgressTrackingService.Models;
using ProgressTrackingService.Tests.Infrastructure;
using Xunit;

namespace ProgressTrackingService.Tests.Biometrics;

public class BiometricsTests : IDisposable
{
    private readonly SqliteTestDbContext _db;
    private readonly GeneralRepo<UserBiometrics> _repo;
    private readonly UnitOfWork _uow;

    public BiometricsTests()
    {
        _db = new SqliteTestDbContext();
        _repo = new GeneralRepo<UserBiometrics>(_db.Context);
        _uow = new UnitOfWork(_db.Context);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    [Fact]
    public async Task UpsertUserBiometrics_InsertsWhenNotExisting_ThenUpdatesOnSubsequentCall()
    {
        // Arrange
        var handler = new UpsertUserBiometricsHandler(_repo, _uow, NullLogger<UpsertUserBiometricsHandler>.Instance);
        var queryHandler = new GetUserHeightHandler(_repo);
        var userId = Guid.NewGuid();

        // Act 1: Initial Insert
        var initialCmd = new UpsertUserBiometricsCommand(userId, 175.5, DateTime.UtcNow);
        var insertResult = await handler.Handle(initialCmd, CancellationToken.None);

        // Assert 1
        Assert.True(insertResult.IsSuccess);
        var heightResult = await queryHandler.Handle(new GetUserHeightQuery(userId), CancellationToken.None);
        Assert.True(heightResult.IsSuccess);
        Assert.Equal(175.5, heightResult.Value);

        // Act 2: Subsequent Update
        var updateCmd = new UpsertUserBiometricsCommand(userId, 180.0, DateTime.UtcNow.AddDays(1));
        var updateResult = await handler.Handle(updateCmd, CancellationToken.None);

        // Assert 2
        Assert.True(updateResult.IsSuccess);
        var updatedHeight = await queryHandler.Handle(new GetUserHeightQuery(userId), CancellationToken.None);
        Assert.True(updatedHeight.IsSuccess);
        Assert.Equal(180.0, updatedHeight.Value);
    }

    [Fact]
    public async Task GetUserHeight_WhenNoBiometricsRow_ReturnsNull()
    {
        // Arrange
        var queryHandler = new GetUserHeightHandler(_repo);
        var userId = Guid.NewGuid();

        // Act
        var result = await queryHandler.Handle(new GetUserHeightQuery(userId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }
}
