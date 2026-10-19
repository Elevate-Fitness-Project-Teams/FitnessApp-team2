using Microsoft.Extensions.Caching.Distributed;

namespace FitnessCalculationService.Tests.Caching;

public class FaultyDistributedCache : IDistributedCache
{
    public byte[]? Get(string key) => throw new TimeoutException("Redis connection timeout.");

    public Task<byte[]?> GetAsync(string key, CancellationToken token = default) =>
        throw new TimeoutException("Redis connection timeout.");

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options) =>
        throw new TimeoutException("Redis connection timeout.");

    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default) =>
        throw new TimeoutException("Redis connection timeout.");

    public void Refresh(string key) => throw new TimeoutException("Redis connection timeout.");

    public Task RefreshAsync(string key, CancellationToken token = default) =>
        throw new TimeoutException("Redis connection timeout.");

    public void Remove(string key) => throw new TimeoutException("Redis connection timeout.");

    public Task RemoveAsync(string key, CancellationToken token = default) =>
        throw new TimeoutException("Redis connection timeout.");
}
