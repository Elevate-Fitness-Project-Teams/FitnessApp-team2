namespace ProgressTrackingService.Features.WorkoutLogs.Queries.GetAvailableAchievements;

public class GetAvailableAchievementResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
}
