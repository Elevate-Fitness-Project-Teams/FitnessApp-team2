namespace ProgressTrackingService.Features.Progress.Commands.LogWeight;

public record LogWeightResponse(
    Guid Id,
    double Weight,
    double? Bmi,
    double DifferenceFromPrevious,
    double TotalWeightLost
);
