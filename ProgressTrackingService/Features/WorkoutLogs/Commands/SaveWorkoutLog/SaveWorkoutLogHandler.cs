using MediatR;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Features.WorkoutLogs.Queries.ValidateWorkoutSession;
using ProgressTrackingService.Features.Streaks.Queries.GetStreak;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.WorkoutLogs.Commands.SaveWorkoutLog;

public class SaveWorkoutLogHandler : IRequestHandler<SaveWorkoutLogCommand, Result<Guid>>
{
    private readonly IGeneralRepo<WorkoutLog> _workoutLogRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public SaveWorkoutLogHandler(
        IGeneralRepo<WorkoutLog> workoutLogRepo,
        IUnitOfWork unitOfWork,
        IMediator mediator)
    {
        _workoutLogRepo = workoutLogRepo;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task<Result<Guid>> Handle(SaveWorkoutLogCommand request, CancellationToken cancellationToken)
    {
        return await  _unitOfWork.ExecuteAsync(async () =>
        {
            var workoutLog = new WorkoutLog
            {
                Id =  Guid.CreateVersion7(),
                UserId = request.UserId,
                WorkoutId = request.WorkoutId,
                SessionId = request.SessionId,
                CompletedAt = request.CompletedAt,
                DurationInMinutes = request.Duration,
                CaloriesBurned = request.CaloriesBurned,
                Difficulty = request.Difficulty,
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

            await _workoutLogRepo.AddAsync(workoutLog, cancellationToken);

            return Result<Guid>.Success(workoutLog.Id);
        }, cancellationToken);
    }
}