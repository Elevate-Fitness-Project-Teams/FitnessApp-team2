namespace MessageBroker.Events;

/// <summary>
/// Published whenever a user is assigned or reassigned to a fitness plan.
/// Consumed by WorkoutService, SmartCoach, and HomeFeed.
/// Wire contract across microservices - DO NOT RENAME.
/// </summary>
public class FitnessPlanAssignedEvent
{
    public Guid UserId { get; set; }
    public string PlanId { get; set; } = string.Empty;
    public string? PreviousPlanId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
}
