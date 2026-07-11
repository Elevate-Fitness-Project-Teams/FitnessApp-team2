using MediatR;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Features.Streaks.Commands.UpdateStreak;
using ProgressTrackingService.Features.WorkoutLogs.Commands.SaveWorkoutLog;
using ProgressTrackingService.Features.WorkoutLogs.Commands.UpdateStatistic;

namespace ProgressTrackingService.Features.WorkoutLogs.Orchestrators.LogWorkout;

public record LogWorkoutOrchestrator(
    Guid UserId,
    Guid WorkoutId,
    string SessionId,
    DateTime CompletedAt,
    int Duration,
    int CaloriesBurned,
    string Difficulty,
    string? Notes,
    int Rating,
    List<LogWorkoutExerciseDto> ExercisesCompleted) : IRequest<Result<Guid>>;

public class LogWorkoutOrchestratorHandler : IRequestHandler<LogWorkoutOrchestrator, Result<Guid>>
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpClientFactory _httpClientFactory;

    public LogWorkoutOrchestratorHandler(
        IMediator mediator,
        IUnitOfWork unitOfWork,
        IHttpClientFactory httpClientFactory)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Result<Guid>> Handle(LogWorkoutOrchestrator request, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("WorkoutService");
        var response = await client.PostAsJsonAsync("api/v1/sessions/complete",
            new { SessionId = request.SessionId, UserId = request.UserId }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return Result<Guid>.Failure(Error.Failure("WORKOUT_SERVICE_ERROR",
                "Failed to complete session in WorkoutService."));
        }

        return await _unitOfWork.ExecuteAsync(async () =>
        {
            var saveCommand = new SaveWorkoutLogCommand
            {
                UserId = request.UserId,
                WorkoutId = request.WorkoutId,
                SessionId = request.SessionId,
                CompletedAt = request.CompletedAt,
                Duration = request.Duration,
                CaloriesBurned = request.CaloriesBurned,
                Difficulty = request.Difficulty,
                Notes = request.Notes,
                Rating = request.Rating,
                ExercisesCompleted = request.ExercisesCompleted.Select(e => new SaveWorkoutLogExerciseDto
                {
                    ExerciseId = e.ExerciseId,
                    Sets = e.Sets,
                    Reps = e.Reps,
                    WeightUsed = e.WeightUsed,
                    Completed = e.Completed
                }).ToList()
            };

            var saveResult = await _mediator.Send(saveCommand, cancellationToken);
            if (!saveResult.IsSuccess)
            {
                return Result<Guid>.Failure(saveResult.Error);
            }

            await _mediator.Send(
                new UpdateStatisticCommand { UserId = request.UserId, CaloriesBurned = request.CaloriesBurned },
                cancellationToken);

            await _mediator.Send(new UpdateStreakCommand { UserId = request.UserId, CompletedAt = request.CompletedAt },
                cancellationToken);

            return Result<Guid>.Success(saveResult.Value);
        }, cancellationToken);
    }
}