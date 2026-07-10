namespace ProgressTrackingService.Features.WorkoutLogs.Orchestrators.LogWorkout;

public class LogWorkoutResponse
{
    public Guid LogId { get; set; }
    public bool StreakUpdated { get; set; }
    public int CurrentStreak { get; set; }
}
