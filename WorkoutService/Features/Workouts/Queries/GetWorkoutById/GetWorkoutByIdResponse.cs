namespace WorkoutService.Features.Workouts.Queries.GetWorkoutById;

public record GetWorkoutByIdResponse(
    Guid WorkoutId,
    string Name,
    int DurationInMinutes,
    string Difficulty,
    IEnumerable<WorkoutExerciseDto> Exercises
);

