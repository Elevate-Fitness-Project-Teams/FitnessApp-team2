namespace FitnessCalculationService.Domain.Entities;

public class CalculatedMetrics
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public double Bmr { get; set; }
    public double Tdee { get; set; }
    public double CalorieTarget { get; set; }
    public DateTime CalculatedAt { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
