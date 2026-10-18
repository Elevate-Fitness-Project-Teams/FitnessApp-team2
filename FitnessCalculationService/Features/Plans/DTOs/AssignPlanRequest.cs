namespace FitnessCalculationService.Features.Plans.DTOs;

public record AssignPlanRequest(Guid? UserId = null, string? Reason = null);
