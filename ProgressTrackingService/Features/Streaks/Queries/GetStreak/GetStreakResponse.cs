namespace ProgressTrackingService.Features.Streaks.Queries.GetStreak;

public class GetStreakResponse
{
    public Guid UserId { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public DateTime? LastWorkoutDate { get; set; }
}
