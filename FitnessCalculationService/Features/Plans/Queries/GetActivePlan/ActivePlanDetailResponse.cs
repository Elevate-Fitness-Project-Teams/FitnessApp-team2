namespace FitnessCalculationService.Features.Plans.Queries.GetActivePlan;

public record ActivePlanDetailResponse(
    Guid AssignmentId,
    Guid PlanConfigId,
    string PlanId,
    string Name,
    string Description,
    string Goal,
    string Status,
    int EstimatedDurationWeeks,
    int WorkoutsPerWeek,
    string ProgramType,
    double MinCalorie,
    double MaxCalorie,
    DateTime AssignedAt,
    bool IsActive
);
