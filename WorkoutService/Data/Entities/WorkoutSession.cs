namespace WorkoutService.Data.Entities;

public class WorkoutSession
{
    public string SessionId { get; set; } = string.Empty;
    public int UserId { get; set; }
    public int WorkoutId { get; set; }
    public DateTime StartedAt { get; set; }
    public string Status { get; set; } = string.Empty;

    public Workout? Workout { get; set; }
}
