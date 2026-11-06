using StackExchange.Redis;

namespace SubscriptionBillingService.Infrastructure.Caching;

public class RedisSubscriptionTierCache : ISubscriptionTierCache
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public RedisSubscriptionTierCache(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = _redis.GetDatabase();
    }

    public async Task SetTierAsync(Guid userId, string tier, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        var key = $"tier:{userId}";
        await _db.StringSetAsync(key, tier, ttl);
    }

    public async Task<string?> GetTierAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var key = $"tier:{userId}";
        var value = await _db.StringGetAsync(key);
        return value.HasValue ? value.ToString() : null;
    }

    public async Task InvalidateTierAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var key = $"tier:{userId}";
        await _db.KeyDeleteAsync(key);
    }
}
