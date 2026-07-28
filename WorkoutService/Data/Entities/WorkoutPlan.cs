namespace WorkoutService.Data.Entities;

public class WorkoutPlan
{
    public Guid Id { get; set; }
    public string ExternalPlanId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;

    public ICollection<Workout> Workouts { get; set; } = new List<Workout>();
}
