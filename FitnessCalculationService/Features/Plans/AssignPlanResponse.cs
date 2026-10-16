namespace FitnessCalculationService.Features.Plans;

public record AssignPlanResponse(
    Guid AssignmentId,
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
    DateTime AssignedAt
);
