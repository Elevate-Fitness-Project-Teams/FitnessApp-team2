namespace WorkoutService.Features.Workouts.Queries.GetWorkouts;

public record GetWorkoutsResponse(
    Guid WorkoutId,
    string Name,
    int DurationInMinutes,
    string Difficulty
);
