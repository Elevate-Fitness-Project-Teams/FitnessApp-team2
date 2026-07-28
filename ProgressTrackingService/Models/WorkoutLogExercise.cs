namespace ProgressTrackingService.Models;

public class WorkoutLogExercise
{
    public Guid Id { get; set; }
    public Guid WorkoutLogId { get; set; }
    public WorkoutLog WorkoutLog { get; set; } = null!;
    public Guid ExerciseId { get; set; }
    public int SetsCompleted { get; set; }
    public int RepsCompleted { get; set; }
    public double WeightUsed { get; set; }
    public bool Completed { get; set; }
}
