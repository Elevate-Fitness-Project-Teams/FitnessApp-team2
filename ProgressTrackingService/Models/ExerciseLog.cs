namespace ProgressTrackingService.Models;

public class ExerciseLog
{
    public Guid Id { get; set; }
    public Guid WorkoutLogId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public int Sets { get; set; }
    public int Reps { get; set; }
    public decimal WeightKg { get; set; }
    public int DurationSeconds { get; set; }
    public string Notes { get; set; } = string.Empty;

    public WorkoutLog WorkoutLog { get; set; } = null!;
}