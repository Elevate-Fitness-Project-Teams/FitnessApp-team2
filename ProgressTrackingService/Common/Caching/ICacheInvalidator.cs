namespace ProgressTrackingService.Common.Caching;

public interface ICacheInvalidator
{
    Task InvalidateUserAsync(Guid userId, CancellationToken ct = default);
    Task InvalidateUserStatsAsync(Guid userId, CancellationToken ct = default);
    Task InvalidateUserAchievementsAsync(Guid userId, CancellationToken ct = default);
    Task InvalidateAchievementsCatalogAsync(CancellationToken ct = default);
    Task<string> GetUserVersionAsync(Guid userId, CancellationToken ct = default);
}
