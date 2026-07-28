namespace WorkoutService.Features.Workouts.Queries.GetWorkoutById;

public record WorkoutExerciseDto(
    Guid ExerciseId,
    string Name,
    int SetsDefault,
    string RepsDefault,
    int RestTimeInSeconds,
    int OrderIndex
);

