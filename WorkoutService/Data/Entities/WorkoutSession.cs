namespace WorkoutService.Data.Entities;

public class WorkoutSession
{
    public string SessionId { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid WorkoutId { get; set; }
    public DateTime StartedAt { get; set; }
    public string Status { get; set; } = string.Empty;

    public Workout? Workout { get; set; }
}
