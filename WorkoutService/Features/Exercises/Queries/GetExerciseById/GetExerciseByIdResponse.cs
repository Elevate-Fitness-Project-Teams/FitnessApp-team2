namespace WorkoutService.Features.Exercises.Queries.GetExerciseById;

public record GetExerciseByIdResponse(
    int ExerciseId,
    string Name,
    string TargetMuscles,
    string Equipment,
    string Description,
    string? VideoUrl
);
