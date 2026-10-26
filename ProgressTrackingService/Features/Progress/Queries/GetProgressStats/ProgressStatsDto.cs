namespace ProgressTrackingService.Features.Progress.Queries.GetProgressStats;

public class ProgressStatsDto
{
    public int TotalWorkouts { get; set; }
    public int TotalCaloriesBurned { get; set; }
    public int TotalMinutes { get; set; }
    public double TotalWeightLost { get; set; }
    public double? CurrentWeight { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public DateTime? LastWorkoutDate { get; set; }
}
