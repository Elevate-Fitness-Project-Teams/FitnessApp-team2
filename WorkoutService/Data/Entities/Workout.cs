namespace WorkoutService.Data.Entities;

public class Workout
{
    public int Id { get; set; }
    public int WorkoutPlanId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public int DurationInMinutes { get; set; }
    public int CaloriesBurn { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsPremium { get; set; }

    public WorkoutPlan? WorkoutPlan { get; set; }
    public ICollection<WorkoutExercise> WorkoutExercises { get; set; } = new List<WorkoutExercise>();
    public ICollection<WorkoutSession> WorkoutSessions { get; set; } = new List<WorkoutSession>();
}
