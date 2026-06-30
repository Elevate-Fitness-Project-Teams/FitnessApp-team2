namespace WorkoutService.Data.Entities;

public class WorkoutExercise
{
    public int Id { get; set; }
    public int WorkoutId { get; set; }
    public int ExerciseId { get; set; }
    public int OrderIndex { get; set; }
    public int SetsDefault { get; set; }
    public string RepsDefault { get; set; } = string.Empty;
    public int RestTimeInSeconds { get; set; }

    public Workout? Workout { get; set; }
    public Exercise? Exercise { get; set; }
}
