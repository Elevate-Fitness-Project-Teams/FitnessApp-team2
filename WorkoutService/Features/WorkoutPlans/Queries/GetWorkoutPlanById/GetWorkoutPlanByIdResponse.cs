namespace WorkoutService.Features.WorkoutPlans.Queries.GetWorkoutPlanById;

public record GetWorkoutPlanByIdResponse(
    int Id,
    string ExternalPlanId,
    string Name,
    string Description,
    string Goal,
    string Status,
    string Difficulty
);
