using MediatR;
using ProgressTrackingService.Common;

namespace ProgressTrackingService.Features.WorkoutLogs.Commands.SaveWorkoutLog;

public class SaveWorkoutLogCommand : IRequest<Result<Guid>>
{
    public Guid UserId { get; set; }
    public Guid WorkoutId { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
    public int Duration { get; set; }
    public int CaloriesBurned { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public int Rating { get; set; }
    public List<SaveWorkoutLogExerciseDto> ExercisesCompleted { get; set; } = new();
}

public class SaveWorkoutLogExerciseDto
{
    public Guid ExerciseId { get; set; }
    public int Sets { get; set; }
    public int Reps { get; set; }
    public double WeightUsed { get; set; }
    public bool Completed { get; set; }
}
