namespace ProgressTrackingService.Common.Caching;

public static class ProgressCacheKeys
{
    public static readonly TimeSpan StatsTtl = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan AchievementsCatalogTtl = TimeSpan.FromHours(24);
    public static readonly TimeSpan UserAchievementsTtl = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan DashboardTtl = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan WeightHistoryTtl = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan UserVersionTtl = TimeSpan.FromDays(30);

    public static readonly string[] PresetDashboardPeriods = { "week", "month", "year", "all" };

    public static string Stats(Guid userId) => $"stats:{userId}";

    public static string AchievementsCatalog() => "achievements:catalog";

    public static string UserAchievements(Guid userId) => $"achievements:{userId}";

    public static string Dashboard(Guid userId, string period) =>
        $"dashboard:{userId}:{period.ToLowerInvariant()}";

    public static string UserVersion(Guid userId) => $"ver:{userId}";

    public static string WeightHistory(Guid userId, string version, int page, int pageSize) =>
        $"weight-history:{userId}:v{version}:p{page}:s{pageSize}";
}
