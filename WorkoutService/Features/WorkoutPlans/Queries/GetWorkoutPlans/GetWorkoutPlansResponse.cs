namespace WorkoutService.Features.WorkoutPlans.Queries.GetWorkoutPlans;

public record GetWorkoutPlansResponse(
    int Id,
    string ExternalPlanId,
    string Name,
    string Description,
    string Goal,
    string Status,
    string Difficulty
);
