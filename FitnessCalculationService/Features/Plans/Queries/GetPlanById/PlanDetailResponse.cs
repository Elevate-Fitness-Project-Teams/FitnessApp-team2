namespace FitnessCalculationService.Features.Plans.Queries.GetPlanById;

public record PlanDetailResponse(
    Guid Id,
    string PlanId,
    string Name,
    string Description,
    string Goal,
    string Status,
    double MinCalorie,
    double MaxCalorie,
    int EstimatedDurationWeeks,
    int WorkoutsPerWeek,
    string ProgramType,
    bool IsActive
);
