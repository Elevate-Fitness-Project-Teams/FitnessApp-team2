namespace WorkoutService.Features.Exercises.Queries.GetExercises;

public record GetExercisesResponse(
    Guid ExerciseId,
    string Name,
    string TargetMuscles,
    string Equipment,
    string Description,
    string? VideoUrl
);

