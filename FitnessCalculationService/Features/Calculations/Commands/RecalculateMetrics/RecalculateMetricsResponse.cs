namespace FitnessCalculationService.Features.Calculations.Commands.RecalculateMetrics;

public record RecalculateMetricsResponse(
    Guid UserId,
    string PreviousStatus,
    string NewStatus,
    double Bmr,
    double Tdee,
    double CalorieTarget,
    bool PlanReassigned,
    string? NewPlanId,
    string? NewPlanName,
    DateTime CalculatedAt
);
