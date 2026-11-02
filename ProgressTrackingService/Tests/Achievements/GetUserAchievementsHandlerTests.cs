using Microsoft.Extensions.Logging.Abstractions;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Caching;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Features.Achievements.DTOs;
using ProgressTrackingService.Features.Achievements.Queries.GetAchievementCatalog;
using ProgressTrackingService.Features.Achievements.Queries.GetUserAchievements;
using ProgressTrackingService.Models;
using ProgressTrackingService.Tests.Caching;
using ProgressTrackingService.Tests.Infrastructure;
using Xunit;

namespace ProgressTrackingService.Tests.Achievements;

public class GetUserAchievementsHandlerTests : IDisposable
{
    private readonly SqliteTestDbContext _db;
    private readonly GeneralRepo<UserAchievement> _repo;
    private readonly TestDistributedCache _cache;
    private readonly RedisCacheService _cacheService;

    public GetUserAchievementsHandlerTests()
    {
        _db = new SqliteTestDbContext();
        _repo = new GeneralRepo<UserAchievement>(_db.Context);
        _cache = new TestDistributedCache();
        _cacheService = new RedisCacheService(_cache, NullLogger<RedisCacheService>.Instance);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    [Fact]
    public async Task Handle_WhenUserHasEarnedAchievements_ReturnsUnlockedOrderedByEarnedAtDesc()
    {
        var userId = Guid.NewGuid();
        var ach1 = new Achievement { Id = Guid.NewGuid(), Code = "WORKOUT_1", Name = "First Workout", Description = "Desc", CriteriaType = "WorkoutsCompleted", Threshold = 1 };
        var ach2 = new Achievement { Id = Guid.NewGuid(), Code = "WORKOUT_5", Name = "Five Workouts", Description = "Desc", CriteriaType = "WorkoutsCompleted", Threshold = 5 };

        _db.Context.Achievements.AddRange(ach1, ach2);
        _db.Context.UserAchievements.AddRange(
            new UserAchievement { Id = Guid.NewGuid(), UserId = userId, AchievementId = ach1.Id, EarnedAt = DateTime.UtcNow.AddDays(-2) },
            new UserAchievement { Id = Guid.NewGuid(), UserId = userId, AchievementId = ach2.Id, EarnedAt = DateTime.UtcNow }
        );
        await _db.Context.SaveChangesAsync();

        var mediator = new MockMediatorBuilder().Build();
        var handler = new GetUserAchievementsHandler(_repo, mediator, _cacheService, NullLogger<GetUserAchievementsHandler>.Instance);

        var result = await handler.Handle(new GetUserAchievementsQuery(userId, IncludeLocked: false), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal("WORKOUT_5", result.Value[0].Code); // Latest earned first
        Assert.Equal("WORKOUT_1", result.Value[1].Code);
        Assert.All(result.Value, a => Assert.True(a.Earned));
    }

    [Fact]
    public async Task Handle_WhenIncludeLockedIsTrue_MergesWithCatalog()
    {
        var userId = Guid.NewGuid();
        var earnedAch = new Achievement { Id = Guid.NewGuid(), Code = "EARNED", Name = "Earned Badge", Description = "Desc", CriteriaType = "WorkoutsCompleted", Threshold = 1 };
        var lockedAch = new Achievement { Id = Guid.NewGuid(), Code = "LOCKED", Name = "Locked Badge", Description = "Desc", CriteriaType = "WorkoutsCompleted", Threshold = 100 };

        _db.Context.Achievements.AddRange(earnedAch, lockedAch);
        _db.Context.UserAchievements.Add(
            new UserAchievement { Id = Guid.NewGuid(), UserId = userId, AchievementId = earnedAch.Id, EarnedAt = DateTime.UtcNow }
        );
        await _db.Context.SaveChangesAsync();

        var catalog = new List<AchievementRuleDto>
        {
            new(earnedAch.Id, earnedAch.Code, earnedAch.Name, earnedAch.Description, string.Empty, earnedAch.CriteriaType, earnedAch.Threshold),
            new(lockedAch.Id, lockedAch.Code, lockedAch.Name, lockedAch.Description, string.Empty, lockedAch.CriteriaType, lockedAch.Threshold)
        };

        var mediator = new MockMediatorBuilder()
            .RegisterHandler<GetAchievementCatalogQuery, Result<List<AchievementRuleDto>>>(q =>
                Task.FromResult(Result.Success(catalog)))
            .Build();

        var handler = new GetUserAchievementsHandler(_repo, mediator, _cacheService, NullLogger<GetUserAchievementsHandler>.Instance);

        var result = await handler.Handle(new GetUserAchievementsQuery(userId, IncludeLocked: true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);

        var earnedItem = result.Value.First(a => a.Code == "EARNED");
        var lockedItem = result.Value.First(a => a.Code == "LOCKED");

        Assert.True(earnedItem.Earned);
        Assert.NotNull(earnedItem.EarnedAt);

        Assert.False(lockedItem.Earned);
        Assert.Null(lockedItem.EarnedAt);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoAchievements_ReturnsEmptyList()
    {
        var userId = Guid.NewGuid();
        var mediator = new MockMediatorBuilder().Build();
        var handler = new GetUserAchievementsHandler(_repo, mediator, _cacheService, NullLogger<GetUserAchievementsHandler>.Instance);

        var result = await handler.Handle(new GetUserAchievementsQuery(userId, IncludeLocked: false), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task Handle_CachesUnlockedAchievements_ServesSubsequentCallsFromCache()
    {
        var userId = Guid.NewGuid();
        var ach = new Achievement { Id = Guid.NewGuid(), Code = "BADGE_1", Name = "Badge", Description = "Desc", CriteriaType = "WorkoutsCompleted", Threshold = 1 };
        _db.Context.Achievements.Add(ach);
        _db.Context.UserAchievements.Add(new UserAchievement { Id = Guid.NewGuid(), UserId = userId, AchievementId = ach.Id, EarnedAt = DateTime.UtcNow });
        await _db.Context.SaveChangesAsync();

        var mediator = new MockMediatorBuilder().Build();
        var handler = new GetUserAchievementsHandler(_repo, mediator, _cacheService, NullLogger<GetUserAchievementsHandler>.Instance);

        // First call caches result
        var result1 = await handler.Handle(new GetUserAchievementsQuery(userId, IncludeLocked: false), CancellationToken.None);
        Assert.True(result1.IsSuccess);
        Assert.Single(result1.Value);

        // Remove from DB to ensure second call hits cache
        _db.Context.UserAchievements.RemoveRange(_db.Context.UserAchievements);
        await _db.Context.SaveChangesAsync();

        var result2 = await handler.Handle(new GetUserAchievementsQuery(userId, IncludeLocked: false), CancellationToken.None);
        Assert.True(result2.IsSuccess);
        Assert.Single(result2.Value);
        Assert.Equal("BADGE_1", result2.Value[0].Code);
    }
}
