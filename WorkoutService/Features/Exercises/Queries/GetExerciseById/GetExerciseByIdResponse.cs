namespace WorkoutService.Features.Exercises.Queries.GetExerciseById;

public record GetExerciseByIdResponse(
    Guid ExerciseId,
    string Name,
    string TargetMuscles,
    string Equipment,
    string Description,
    string? VideoUrl
);

