using Microsoft.Extensions.Logging;

namespace ProgressTrackingService.Common.Caching;

public class CacheInvalidator : ICacheInvalidator
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheInvalidator> _logger;

    public CacheInvalidator(ICacheService cacheService, ILogger<CacheInvalidator> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task InvalidateUserAsync(Guid userId, CancellationToken ct = default)
    {
        try
        {
            await InvalidateUserStatsAsync(userId, ct);
            await InvalidateUserAchievementsAsync(userId, ct);

            // Rotate version stamp to invalidate all paged weight-history cached entries without KEYS/SCAN
            var newVersion = Guid.NewGuid().ToString("N");
            await _cacheService.SetAsync(
                ProgressCacheKeys.UserVersion(userId),
                newVersion,
                ProgressCacheKeys.UserVersionTtl,
                ct);

            _logger.LogInformation("Cache invalidated and version rotated for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to completely invalidate cache for user {UserId}. Continuing execution.", userId);
        }
    }

    public async Task InvalidateUserStatsAsync(Guid userId, CancellationToken ct = default)
    {
        try
        {
            await _cacheService.RemoveAsync(ProgressCacheKeys.Stats(userId), ct);

            foreach (var period in ProgressCacheKeys.PresetDashboardPeriods)
            {
                await _cacheService.RemoveAsync(ProgressCacheKeys.Dashboard(userId, period), ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to invalidate stats cache for user {UserId}", userId);
        }
    }

    public async Task InvalidateUserAchievementsAsync(Guid userId, CancellationToken ct = default)
    {
        try
        {
            await _cacheService.RemoveAsync(ProgressCacheKeys.UserAchievements(userId), ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to invalidate achievements cache for user {UserId}", userId);
        }
    }

    public async Task InvalidateAchievementsCatalogAsync(CancellationToken ct = default)
    {
        try
        {
            await _cacheService.RemoveAsync(ProgressCacheKeys.AchievementsCatalog(), ct);
            _logger.LogInformation("Achievements catalog cache invalidated.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to invalidate achievements catalog cache.");
        }
    }

    public async Task<string> GetUserVersionAsync(Guid userId, CancellationToken ct = default)
    {
        try
        {
            var key = ProgressCacheKeys.UserVersion(userId);
            var version = await _cacheService.GetAsync<string>(key, ct);
            if (!string.IsNullOrWhiteSpace(version))
            {
                return version;
            }

            var newVersion = Guid.NewGuid().ToString("N");
            await _cacheService.SetAsync(key, newVersion, ProgressCacheKeys.UserVersionTtl, ct);
            return newVersion;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get or initialize user version for {UserId}. Generating transient version.", userId);
            return Guid.NewGuid().ToString("N");
        }
    }
}
