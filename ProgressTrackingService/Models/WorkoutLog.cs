namespace ProgressTrackingService.Models;

public class WorkoutLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid WorkoutId { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public int DurationInMinutes { get; set; }
    public int CaloriesBurned { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Notes { get; set; }
    public DateTime CompletedAt { get; set; }

    public ICollection<WorkoutLogExercise> Exercises { get; set; } = new List<WorkoutLogExercise>();
}