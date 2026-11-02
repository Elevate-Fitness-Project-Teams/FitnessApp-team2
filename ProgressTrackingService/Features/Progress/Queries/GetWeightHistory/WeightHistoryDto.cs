namespace ProgressTrackingService.Features.Progress.Queries.GetWeightHistory;

public class WeightHistoryDto
{
    public Guid Id { get; set; }
    public double Weight { get; set; }
    public DateTime Date { get; set; }
    public string? Notes { get; set; }
}
