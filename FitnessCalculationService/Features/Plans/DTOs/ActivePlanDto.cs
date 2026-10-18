namespace FitnessCalculationService.Features.Plans.DTOs;

public record ActivePlanDto(
    Guid AssignmentId,
    Guid PlanConfigId,
    string PlanId,
    DateTime AssignedAt
);
