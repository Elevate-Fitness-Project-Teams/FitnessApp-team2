namespace WorkoutService.Features.Workouts.Queries.GetWorkoutById;

public record GetWorkoutByIdResponse(
    int WorkoutId,
    string Name,
    int DurationInMinutes,
    string Difficulty,
    IEnumerable<WorkoutExerciseDto> Exercises
);
