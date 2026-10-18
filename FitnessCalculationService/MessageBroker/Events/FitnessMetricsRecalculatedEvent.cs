namespace MessageBroker.Events;

/// <summary>
/// Published whenever user calculated metrics are updated or recalculated.
/// Consumed by NutritionService, SmartCoach, and HomeFeed recommendation cache.
/// Wire contract across microservices - DO NOT RENAME.
/// </summary>
public class FitnessMetricsRecalculatedEvent
{
    public Guid UserId { get; set; }
    public double Bmr { get; set; }
    public double Tdee { get; set; }
    public double CalorieTarget { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PreviousStatus { get; set; }
    public DateTime CalculatedAt { get; set; }
}
