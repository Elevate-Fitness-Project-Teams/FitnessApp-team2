namespace MessageBroker.Events;

/// <summary>
/// Event broadcast by FitnessCalculationService when user submits fitness stats.
/// Consumed by ProgressTrackingService to project height biometrics for BMI calculations.
/// Wire contract across microservices - DO NOT RENAME.
/// </summary>
public class FitnessStatsSubmittedEvent
{
    public Guid UserId { get; set; }
    public double HeightCm { get; set; }
    public double WeightKg { get; set; }
    public DateTime RecordedAt { get; set; }
}
