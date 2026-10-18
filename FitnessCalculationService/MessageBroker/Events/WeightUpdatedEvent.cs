namespace MessageBroker.Events;

/// <summary>
/// Event broadcast by ProgressTrackingService when an athlete logs a new weight entry.
/// Consumed by FitnessCalculationService to trigger adaptive recalculation.
/// Wire contract across microservices - DO NOT RENAME.
/// </summary>
public class WeightUpdatedEvent
{
    public Guid UserId { get; set; }
    public double NewWeight { get; set; }
    public DateTime RecordedAt { get; set; }
}
