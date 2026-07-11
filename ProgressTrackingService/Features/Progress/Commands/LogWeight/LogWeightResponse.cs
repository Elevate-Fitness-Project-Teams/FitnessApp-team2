namespace ProgressTrackingService.Features.Progress.Commands.LogWeight;

public class LogWeightResponse
{
    public double DifferenceFromPrevious { get; set; }
    public double TotalWeightLost { get; set; }
}
