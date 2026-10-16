namespace MessageBroker.Events;

public class PlanAssignedEvent
{
    public Guid UserId { get; set; }
    public string PlanId { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
}
