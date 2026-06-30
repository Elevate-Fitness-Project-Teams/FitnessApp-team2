namespace WorkoutService.Features.Workouts.Queries.GetWorkouts;

public record GetWorkoutsResponse(
    int WorkoutId,
    string Name,
    int DurationInMinutes,
    string Difficulty
);
