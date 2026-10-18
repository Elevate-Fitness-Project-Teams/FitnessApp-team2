namespace FitnessCalculationService.Features.Plans.Queries.GetPlanConfigs;

public record PlanConfigListItem(
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
