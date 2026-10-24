using Microsoft.Extensions.Logging.Abstractions;
using ProgressTrackingService.Common.Caching;
using Xunit;

namespace ProgressTrackingService.Tests.Caching;

public class CacheInvalidatorTests
{
    [Fact]
    public async Task InvalidateUserAsync_EvictsStatsAchievementsPresetsAndRotatesVersion()
    {
        // Arrange
        var testCache = new TestDistributedCache();
        var cacheService = new RedisCacheService(testCache, NullLogger<RedisCacheService>.Instance);
        var invalidator = new CacheInvalidator(cacheService, NullLogger<CacheInvalidator>.Instance);

        var userId = Guid.NewGuid();
        var initialVersion = await invalidator.GetUserVersionAsync(userId);

        await cacheService.SetAsync(ProgressCacheKeys.Stats(userId), "stats-payload");
        await cacheService.SetAsync(ProgressCacheKeys.UserAchievements(userId), "achievements-payload");
        foreach (var period in ProgressCacheKeys.PresetDashboardPeriods)
        {
            await cacheService.SetAsync(ProgressCacheKeys.Dashboard(userId, period), $"{period}-payload");
        }

        // Act
        await invalidator.InvalidateUserAsync(userId);

        // Assert
        Assert.Null(await cacheService.GetAsync<string>(ProgressCacheKeys.Stats(userId)));
        Assert.Null(await cacheService.GetAsync<string>(ProgressCacheKeys.UserAchievements(userId)));
        foreach (var period in ProgressCacheKeys.PresetDashboardPeriods)
        {
            Assert.Null(await cacheService.GetAsync<string>(ProgressCacheKeys.Dashboard(userId, period)));
        }

        var newVersion = await invalidator.GetUserVersionAsync(userId);
        Assert.NotNull(newVersion);
        Assert.NotEqual(initialVersion, newVersion);
    }

    [Fact]
    public async Task InvalidateAchievementsCatalogAsync_EvictsCatalogKey()
    {
        // Arrange
        var testCache = new TestDistributedCache();
        var cacheService = new RedisCacheService(testCache, NullLogger<RedisCacheService>.Instance);
        var invalidator = new CacheInvalidator(cacheService, NullLogger<CacheInvalidator>.Instance);

        await cacheService.SetAsync(ProgressCacheKeys.AchievementsCatalog(), "master-catalog");

        // Act
        await invalidator.InvalidateAchievementsCatalogAsync();

        // Assert
        Assert.Null(await cacheService.GetAsync<string>(ProgressCacheKeys.AchievementsCatalog()));
    }

    [Fact]
    public async Task GetUserVersionAsync_WhenVersionNotExists_GeneratesAndCachesVersion()
    {
        // Arrange
        var testCache = new TestDistributedCache();
        var cacheService = new RedisCacheService(testCache, NullLogger<RedisCacheService>.Instance);
        var invalidator = new CacheInvalidator(cacheService, NullLogger<CacheInvalidator>.Instance);
        var userId = Guid.NewGuid();

        // Act
        var version1 = await invalidator.GetUserVersionAsync(userId);
        var version2 = await invalidator.GetUserVersionAsync(userId);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(version1));
        Assert.Equal(version1, version2);
    }

    [Fact]
    public async Task FaultyCache_WhenRedisFails_InvalidatorDegradesGracefullyWithoutThrowing()
    {
        // Arrange
        var faultyCache = new FaultyDistributedCache();
        var cacheService = new RedisCacheService(faultyCache, NullLogger<RedisCacheService>.Instance);
        var invalidator = new CacheInvalidator(cacheService, NullLogger<CacheInvalidator>.Instance);
        var userId = Guid.NewGuid();

        // Act & Assert
        var userEx = await Record.ExceptionAsync(() => invalidator.InvalidateUserAsync(userId));
        Assert.Null(userEx);

        var catalogEx = await Record.ExceptionAsync(() => invalidator.InvalidateAchievementsCatalogAsync());
        Assert.Null(catalogEx);

        var fallbackVersion = await invalidator.GetUserVersionAsync(userId);
        Assert.False(string.IsNullOrWhiteSpace(fallbackVersion));
    }
}
