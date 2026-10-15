using FitnessCalculationService.Domain.Enums;

namespace FitnessCalculationService.Domain.Entities;

public class CalculatedMetrics
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public double Bmr { get; set; }
    public double Tdee { get; set; }
    public double CalorieTarget { get; set; }
    public CalorieStatus Status { get; set; }
    public DateTime CalculatedAt { get; set; }
    public Guid? SourceStatsId { get; set; }
    public DateTime? LastWeightEventAt { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
