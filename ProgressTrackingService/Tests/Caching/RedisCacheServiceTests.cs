using Microsoft.Extensions.Logging.Abstractions;
using ProgressTrackingService.Common.Caching;
using Xunit;

namespace ProgressTrackingService.Tests.Caching;

public class RedisCacheServiceTests
{
    private record SampleCachePayload(string Id, string Name, double Value);

    [Fact]
    public async Task SetAndGetAsync_ValidPayload_ReturnsDeserializedObject()
    {
        // Arrange
        var testCache = new TestDistributedCache();
        var cacheService = new RedisCacheService(testCache, NullLogger<RedisCacheService>.Instance);
        var payload = new SampleCachePayload("user-123", "Workout Log", 450.5);

        // Act
        await cacheService.SetAsync("sample:key", payload, TimeSpan.FromMinutes(5));
        var cachedResult = await cacheService.GetAsync<SampleCachePayload>("sample:key");

        // Assert
        Assert.NotNull(cachedResult);
        Assert.Equal("user-123", cachedResult.Id);
        Assert.Equal("Workout Log", cachedResult.Name);
        Assert.Equal(450.5, cachedResult.Value);
    }

    [Fact]
    public async Task GetAsync_KeyDoesNotExist_ReturnsNull()
    {
        // Arrange
        var testCache = new TestDistributedCache();
        var cacheService = new RedisCacheService(testCache, NullLogger<RedisCacheService>.Instance);

        // Act
        var result = await cacheService.GetAsync<SampleCachePayload>("non-existent-key");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveAsync_ExistingKey_EvictsFromStore()
    {
        // Arrange
        var testCache = new TestDistributedCache();
        var cacheService = new RedisCacheService(testCache, NullLogger<RedisCacheService>.Instance);
        var payload = new SampleCachePayload("key1", "Item", 100);

        await cacheService.SetAsync("evict:key", payload);

        // Act
        await cacheService.RemoveAsync("evict:key");
        var result = await cacheService.GetAsync<SampleCachePayload>("evict:key");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FaultyCache_WhenRedisFails_DoesNotThrowAndFallsBackGracefully()
    {
        // Arrange
        var faultyCache = new FaultyDistributedCache();
        var cacheService = new RedisCacheService(faultyCache, NullLogger<RedisCacheService>.Instance);
        var payload = new SampleCachePayload("key1", "Item", 100);

        // Act & Assert - None of these should throw
        var getResult = await cacheService.GetAsync<SampleCachePayload>("faulty:key");
        Assert.Null(getResult);

        var setException = await Record.ExceptionAsync(() =>
            cacheService.SetAsync("faulty:key", payload));
        Assert.Null(setException);

        var removeException = await Record.ExceptionAsync(() =>
            cacheService.RemoveAsync("faulty:key"));
        Assert.Null(removeException);
    }
}
