using System.Net.Http.Json;
using MediatR;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Features.Streaks.Queries.GetStreak;
using ProgressTrackingService.Features.Streaks.Commands.UpdateStreak;
using ProgressTrackingService.Features.WorkoutLogs.Commands.SaveWorkoutLog;
using ProgressTrackingService.Features.WorkoutLogs.Commands.UpdateSession;
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
    List<LogWorkoutExerciseDto> ExercisesCompleted) : IRequest<Result<LogWorkoutResponse>>;

public class LogWorkoutOrchestratorHandler : IRequestHandler<LogWorkoutOrchestrator, Result<LogWorkoutResponse>>
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpClientFactory _httpClientFactory;

    public LogWorkoutOrchestratorHandler(IMediator mediator, IUnitOfWork unitOfWork,
        IHttpClientFactory httpClientFactory)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Result<LogWorkoutResponse>> Handle(LogWorkoutOrchestrator request,
        CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            var client = _httpClientFactory.CreateClient("WorkoutService");
            var response = await client.PostAsJsonAsync("api/v1/sessions/complete",
                new { SessionId = request.SessionId, UserId = request.UserId }, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Result<LogWorkoutResponse>.Failure(Error.Failure("WORKOUT_SERVICE_ERROR",
                    "Failed to complete session in WorkoutService."));
            }

            var oldStreakResult = await _mediator.Send(new GetStreakQuery { UserId = request.UserId }, cancellationToken);
            var oldStreak = oldStreakResult.IsSuccess ? oldStreakResult.Value : null;
            int previousStreak = oldStreak?.CurrentStreak ?? 0;
            DateTime? previousDate = oldStreak?.LastWorkoutDate;

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
                return Result<LogWorkoutResponse>.Failure(saveResult.Error);
            }

            await _mediator.Send(
                new UpdateStatisticCommand { UserId = request.UserId, CaloriesBurned = request.CaloriesBurned },
                cancellationToken);

            await _mediator.Send(new UpdateStreakCommand { UserId = request.UserId, CompletedAt = request.CompletedAt },
                cancellationToken);

            var newStreakResult =
                await _mediator.Send(new GetStreakQuery { UserId = request.UserId }, cancellationToken);
            var newStreak = newStreakResult.IsSuccess ? newStreakResult.Value : null;

            var result = new LogWorkoutResponse
            {
                LogId = saveResult.Value,
                CurrentStreak = newStreak?.CurrentStreak ?? 0,
                StreakUpdated = false
            };

            if (newStreak != null && (previousDate == null || newStreak.CurrentStreak > previousStreak ||
                                      (newStreak.CurrentStreak == 1 && previousStreak > 1)))
            {
                result.StreakUpdated = true;
            }

            return Result<LogWorkoutResponse>.Success(result);
        }, cancellationToken);
    }
}