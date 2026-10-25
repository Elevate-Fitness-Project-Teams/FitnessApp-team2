using Microsoft.Extensions.Logging.Abstractions;
using ProgressTrackingService.Common.Caching;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Features.Achievements.Queries.GetAchievementCatalog;
using ProgressTrackingService.Models;
using ProgressTrackingService.Tests.Caching;
using ProgressTrackingService.Tests.Infrastructure;
using Xunit;

namespace ProgressTrackingService.Tests.Achievements;

public class GetAchievementCatalogQueryTests : IDisposable
{
    private readonly SqliteTestDbContext _db;
    private readonly TestDistributedCache _testCache;
    private readonly RedisCacheService _cacheService;
    private readonly GetAchievementCatalogHandler _handler;

    public GetAchievementCatalogQueryTests()
    {
        _db = new SqliteTestDbContext();
        _testCache = new TestDistributedCache();
        _cacheService = new RedisCacheService(_testCache, NullLogger<RedisCacheService>.Instance);
        var repo = new GeneralRepo<Achievement>(_db.Context);

        _handler = new GetAchievementCatalogHandler(repo, _cacheService);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    [Fact]
    public async Task Handle_WhenCacheMiss_ReadsFromDbAndPopulatesCache()
    {
        // Arrange
        var achievement = new Achievement
        {
            Id = Guid.NewGuid(),
            Code = "FIRST_WORKOUT",
            Name = "First Step",
            Description = "Completed first workout",
            IconUrl = "https://cdn.example.com/icon.png",
            CriteriaType = "TotalWorkouts",
            Threshold = 1
        };
        _db.Context.Achievements.Add(achievement);
        await _db.Context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(new GetAchievementCatalogQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal("FIRST_WORKOUT", result.Value[0].Code);

        // Verify it was cached
        var cached = await _cacheService.GetAsync<List<Features.Achievements.DTOs.AchievementRuleDto>>(
            ProgressCacheKeys.AchievementsCatalog());
        Assert.NotNull(cached);
        Assert.Single(cached);
        Assert.Equal("FIRST_WORKOUT", cached[0].Code);
    }

    [Fact]
    public async Task Handle_WhenCacheHit_ReturnsCachedValueWithoutHittingDb()
    {
        // Arrange
        var cachedRules = new List<Features.Achievements.DTOs.AchievementRuleDto>
        {
            new(Guid.NewGuid(), "CACHED_BADGE", "Cached Badge", "From Cache", "", "TotalWorkouts", 5)
        };
        await _cacheService.SetAsync(ProgressCacheKeys.AchievementsCatalog(), cachedRules);

        // Act
        var result = await _handler.Handle(new GetAchievementCatalogQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal("CACHED_BADGE", result.Value[0].Code);
    }
}
