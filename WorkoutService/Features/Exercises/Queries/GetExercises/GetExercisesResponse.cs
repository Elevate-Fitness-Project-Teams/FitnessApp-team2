namespace WorkoutService.Features.Exercises.Queries.GetExercises;

public record GetExercisesResponse(
    int ExerciseId,
    string Name,
    string TargetMuscles,
    string Equipment,
    string Description,
    string? VideoUrl
);
