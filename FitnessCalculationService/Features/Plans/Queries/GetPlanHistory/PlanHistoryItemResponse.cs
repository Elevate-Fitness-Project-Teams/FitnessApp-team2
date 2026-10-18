namespace FitnessCalculationService.Features.Plans.Queries.GetPlanHistory;

public record PlanHistoryItemResponse(
    Guid Id,
    string PlanId,
    string? PreviousPlanId,
    string Goal,
    string Status,
    string? PreviousStatus,
    double Bmr,
    double Tdee,
    double CalorieTarget,
    Guid? AssignedPlanId,
    string TriggeredBy,
    string Reason,
    DateTime AssignedAt
);
