namespace WorkoutService.Features.Workouts.Queries.GetWorkoutById;

public record WorkoutExerciseDto(
    int ExerciseId,
    string Name,
    int SetsDefault,
    string RepsDefault,
    int RestTimeInSeconds,
    int OrderIndex
);
