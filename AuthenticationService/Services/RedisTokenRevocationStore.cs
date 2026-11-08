using StackExchange.Redis;

namespace AuthenticationService.Services;

public class RedisTokenRevocationStore : ITokenRevocationStore
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisTokenRevocationStore> _logger;

    public const string JtiKeyPrefix = "auth:revoked:jti:";
    public const string UserCutoffKeyPrefix = "auth:revoked-before:";

    public RedisTokenRevocationStore(IConnectionMultiplexer redis, ILogger<RedisTokenRevocationStore> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    private IDatabase Database => _redis.GetDatabase();

    public async Task RevokeTokenAsync(string jti, TimeSpan ttl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(jti))
            return;

        if (ttl <= TimeSpan.Zero)
        {
            _logger.LogDebug("Token {Jti} is already expired. Skipping Redis denylist insertion.", jti);
            return;
        }

        try
        {
            var key = $"{JtiKeyPrefix}{jti}";
            await Database.StringSetAsync(key, "1", ttl);
            _logger.LogInformation("Revoked token {Jti} with TTL {TtlSeconds}s in Redis.", jti, ttl.TotalSeconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write revoked token {Jti} to Redis.", jti);
            throw;
        }
    }

    public async Task RevokeAllUserTokensAsync(Guid userId, DateTimeOffset cutoffUtc, TimeSpan ttl, CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            return;

        if (ttl <= TimeSpan.Zero)
        {
            ttl = TimeSpan.FromMinutes(15);
        }

        try
        {
            var key = $"{UserCutoffKeyPrefix}{userId}";
            var cutoffUnix = cutoffUtc.ToUnixTimeSeconds().ToString();
            await Database.StringSetAsync(key, cutoffUnix, ttl);
            _logger.LogInformation("Revoked all active tokens for User {UserId} before {Cutoff} with TTL {TtlSeconds}s.",
                userId, cutoffUtc, ttl.TotalSeconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write user revocation cutoff for User {UserId} to Redis.", userId);
            throw;
        }
    }

    public async Task<bool> IsTokenRevokedAsync(string? jti, Guid? userId = null, long? issuedAtUnix = null, CancellationToken ct = default)
    {
        var hasJti = !string.IsNullOrWhiteSpace(jti);
        var hasUser = userId.HasValue && userId.Value != Guid.Empty;

        if (!hasJti && !hasUser)
            return false;

        try
        {
            if (hasJti && hasUser)
            {
                RedisKey[] keys =
                {
                    $"{JtiKeyPrefix}{jti}",
                    $"{UserCutoffKeyPrefix}{userId!.Value}"
                };

                var values = await Database.StringGetAsync(keys);

                // 1. Check if the specific jti is revoked
                if (values[0].HasValue)
                {
                    _logger.LogDebug("Token {Jti} is revoked in Redis denylist.", jti);
                    return true;
                }

                // 2. Check if the user has an active revocation cutoff
                if (values[1].HasValue && long.TryParse(values[1], out var cutoffUnix))
                {
                    if (!issuedAtUnix.HasValue || issuedAtUnix.Value <= cutoffUnix)
                    {
                        _logger.LogDebug("Token for User {UserId} issued at {IssuedAt} is before cutoff {Cutoff}.",
                            userId, issuedAtUnix, cutoffUnix);
                        return true;
                    }
                }

                return false;
            }

            if (hasJti)
            {
                var val = await Database.StringGetAsync($"{JtiKeyPrefix}{jti}");
                return val.HasValue;
            }

            if (hasUser)
            {
                var val = await Database.StringGetAsync($"{UserCutoffKeyPrefix}{userId!.Value}");
                if (val.HasValue && long.TryParse(val, out var cutoffUnix))
                {
                    return !issuedAtUnix.HasValue || issuedAtUnix.Value <= cutoffUnix;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking token revocation in Redis. Failing open to allow local validation.");
            return false;
        }
    }
}
