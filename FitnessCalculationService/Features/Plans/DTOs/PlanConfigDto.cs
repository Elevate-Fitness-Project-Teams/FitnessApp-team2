using FitnessCalculationService.Domain.Enums;

namespace FitnessCalculationService.Features.Plans.DTOs;

public record PlanConfigDto(
    Guid Id,
    string PlanId,
    string Name,
    string Description,
    FitnessGoal Goal,
    CalorieStatus Status,
    double MinCalorie,
    double MaxCalorie,
    int EstimatedDurationWeeks,
    int WorkoutsPerWeek,
    string ProgramType
);
