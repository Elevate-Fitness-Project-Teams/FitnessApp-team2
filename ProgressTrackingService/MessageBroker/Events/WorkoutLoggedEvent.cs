namespace MessageBroker.Events;

/// <summary>
/// Event broadcast by ProgressTrackingService when an athlete logs a completed workout session.
/// Wire contract across microservices - DO NOT RENAME.
/// </summary>
public class WorkoutLoggedEvent
{
    public Guid UserId { get; set; }
    public Guid LogId { get; set; }
    public Guid WorkoutId { get; set; }
    public int CaloriesBurned { get; set; }
    public int DurationInMinutes { get; set; }
    public DateTime CompletedAt { get; set; }
}
