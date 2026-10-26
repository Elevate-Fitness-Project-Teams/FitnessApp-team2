using MessageBroker.Events;
using Microsoft.Extensions.Logging.Abstractions;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Features.Achievements.Commands.AwardAchievement;
using ProgressTrackingService.Models;
using ProgressTrackingService.Tests.Infrastructure;
using Xunit;

namespace ProgressTrackingService.Tests.Achievements;

public class AwardAchievementHandlerTests : IDisposable
{
    private readonly SqliteTestDbContext _db;
    private readonly TestPublishEndpoint _publishEndpoint;
    private readonly AwardAchievementHandler _handler;

    public AwardAchievementHandlerTests()
    {
        _db = new SqliteTestDbContext();
        _publishEndpoint = new TestPublishEndpoint();

        var repo = new GeneralRepo<UserAchievement>(_db.Context);
        var uow = new UnitOfWork(_db.Context);

        _handler = new AwardAchievementHandler(
            repo,
            uow,
            _publishEndpoint,
            NullLogger<AwardAchievementHandler>.Instance);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    [Fact]
    public async Task Handle_WhenAchievementNotYetEarned_InsertsRecordAndPublishesEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var achievementId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;

        _db.Context.Achievements.Add(new Achievement
        {
            Id = achievementId,
            Code = "FIRST_WORKOUT",
            Name = "First Step",
            CriteriaType = "TotalWorkouts",
            Threshold = 1
        });
        await _db.Context.SaveChangesAsync();

        var cmd = new AwardAchievementCommand(userId, achievementId, "First Step", timestamp);

        // Act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        var saved = _db.Context.UserAchievements.FirstOrDefault(ua => ua.UserId == userId && ua.AchievementId == achievementId);
        Assert.NotNull(saved);

        Assert.Single(_publishEndpoint.PublishedMessages);
        var published = Assert.IsType<AchievementEarnedEvent>(_publishEndpoint.PublishedMessages[0]);
        Assert.Equal(userId, published.UserId);
        Assert.Equal(achievementId, published.AchievementId);
        Assert.Equal("First Step", published.AchievementName);
    }

    [Fact]
    public async Task Handle_WhenAchievementAlreadyEarned_SkipsInsertAndDoesNotPublishDuplicateEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var achievementId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;

        _db.Context.Achievements.Add(new Achievement
        {
            Id = achievementId,
            Code = "FIRST_WORKOUT",
            Name = "First Step",
            CriteriaType = "TotalWorkouts",
            Threshold = 1
        });

        _db.Context.UserAchievements.Add(new UserAchievement
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AchievementId = achievementId,
            EarnedAt = timestamp.AddHours(-1)
        });
        await _db.Context.SaveChangesAsync();

        var cmd = new AwardAchievementCommand(userId, achievementId, "First Step", timestamp);

        // Act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(_publishEndpoint.PublishedMessages);
    }
}
