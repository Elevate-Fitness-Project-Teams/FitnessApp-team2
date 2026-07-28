namespace WorkoutService.Features.WorkoutPlans.Queries.GetWorkoutPlans;

public record GetWorkoutPlansResponse(
    Guid Id,
    string ExternalPlanId,
    string Name,
    string Description,
    string Goal,
    string Status,
    string Difficulty
);

