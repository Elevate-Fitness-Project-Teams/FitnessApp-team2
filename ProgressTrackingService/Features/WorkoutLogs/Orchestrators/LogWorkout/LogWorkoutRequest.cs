namespace ProgressTrackingService.Features.WorkoutLogs.Orchestrators.LogWorkout;

public class LogWorkoutRequest
{
    public Guid WorkoutId { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
    public int Duration { get; set; }
    public int CaloriesBurned { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public int Rating { get; set; }
    public List<LogWorkoutExerciseDto> ExercisesCompleted { get; set; } = new();
}

public class LogWorkoutExerciseDto
{
    public Guid ExerciseId { get; set; }
    public int Sets { get; set; }
    public int Reps { get; set; }
    public double WeightUsed { get; set; }
    public bool Completed { get; set; }
}
