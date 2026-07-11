namespace ProgressTrackingService.MessageBroker.Events;

public class WeightUpdatedEvent
{
    public Guid UserId { get; set; }
    public double NewWeight { get; set; }
    public DateTime RecordedAt { get; set; }
}
