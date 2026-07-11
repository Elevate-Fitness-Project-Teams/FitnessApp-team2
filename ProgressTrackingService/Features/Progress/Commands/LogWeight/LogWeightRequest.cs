namespace ProgressTrackingService.Features.Progress.Commands.LogWeight;

public class LogWeightRequest
{
    public double Weight { get; set; }
    public DateTime Date { get; set; }
    public string? Notes { get; set; }
}
