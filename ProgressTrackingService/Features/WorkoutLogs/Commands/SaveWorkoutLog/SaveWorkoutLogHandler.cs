using MediatR;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.WorkoutLogs.Commands.SaveWorkoutLog;

public class SaveWorkoutLogHandler : IRequestHandler<SaveWorkoutLogCommand, Result<Guid>>
{
    private readonly IGeneralRepo<WorkoutLog> _workoutLogRepo;

    public SaveWorkoutLogHandler(IGeneralRepo<WorkoutLog> workoutLogRepo)
    {
        _workoutLogRepo = workoutLogRepo;
    }

    public async Task<Result<Guid>> Handle(SaveWorkoutLogCommand request, CancellationToken ct)
    {
        var workoutLog = new WorkoutLog
        {
            Id = Guid.CreateVersion7(),
            UserId = request.UserId,
            WorkoutId = request.WorkoutId,
            SessionId = request.SessionId,
            CompletedAt = request.CompletedAt,
            DurationInMinutes = request.Duration,
            CaloriesBurned = request.CaloriesBurned,
            Difficulty = Enum.Parse<WorkoutDifficulty>(request.Difficulty, true),
            Notes = request.Notes,
            Rating = request.Rating,
            Exercises = request.ExercisesCompleted.Select(e => new WorkoutLogExercise
            {
                ExerciseId = e.ExerciseId,
                SetsCompleted = e.Sets,
                RepsCompleted = e.Reps,
                WeightUsed = e.WeightUsed,
                Completed = e.Completed
            }).ToList()
        };
        await _workoutLogRepo.AddAsync(workoutLog, ct);
        return Result<Guid>.Success(workoutLog.Id);
    }
}
