using MassTransit;
using MediatR;
using MessageBroker.Events;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Caching;
using ProgressTrackingService.Common.Grpc;
using ProgressTrackingService.Features.Achievements.DTOs;
using ProgressTrackingService.Features.Achievements.Orchestrators.EvaluateAchievements;
using ProgressTrackingService.Features.Streaks.Commands.UpdateStreak;
using ProgressTrackingService.Features.WorkoutLogs.Commands.SaveWorkoutLog;
using ProgressTrackingService.Features.WorkoutLogs.Commands.UpdateStatistic;
using ProgressTrackingService.Features.WorkoutLogs.DTOs;
using ProgressTrackingService.Features.WorkoutLogs.Queries.GetWorkoutLogBySession;

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
    private readonly IWorkoutSessionClient _sessionClient;
    private readonly ICacheInvalidator _cacheInvalidator;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<LogWorkoutOrchestratorHandler> _logger;

    public LogWorkoutOrchestratorHandler(
        IMediator mediator,
        IWorkoutSessionClient sessionClient,
        ICacheInvalidator cacheInvalidator,
        IPublishEndpoint publishEndpoint,
        ILogger<LogWorkoutOrchestratorHandler> logger)
    {
        _mediator = mediator;
        _sessionClient = sessionClient;
        _cacheInvalidator = cacheInvalidator;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<Result<LogWorkoutResponse>> Handle(LogWorkoutOrchestrator request, CancellationToken cancellationToken)
    {
        // 1. Idempotency check: verify if session already logged locally
        var existingLogIdResult = await _mediator.Send(
            new GetWorkoutLogBySessionQuery(request.SessionId, request.UserId),
            cancellationToken);

        if (existingLogIdResult.IsSuccess && existingLogIdResult.Value.HasValue)
        {
            _logger.LogInformation("Workout session {SessionId} already logged with id {LogId}. Returning idempotent response.",
                request.SessionId, existingLogIdResult.Value.Value);

            return Result.Success(new LogWorkoutResponse(
                existingLogIdResult.Value.Value,
                StreakUpdated: false,
                CurrentStreak: 0,
                NewAchievements: new List<EarnedAchievementDto>()));
        }

        // 2. Complete session via gRPC in WorkoutService
        var completeSessionResult = await _sessionClient.CompleteSessionAsync(
            request.SessionId,
            request.UserId,
            cancellationToken);

        if (!completeSessionResult.IsSuccess)
        {
            return Result.Failure<LogWorkoutResponse>(completeSessionResult.Error);
        }

        // 3. Save workout log aggregate
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
            return Result.Failure<LogWorkoutResponse>(saveResult.Error);
        }

        // 4. Update statistics atomically
        await _mediator.Send(new UpdateStatisticCommand
        {
            UserId = request.UserId,
            CaloriesBurned = request.CaloriesBurned,
            DurationInMinutes = request.Duration
        }, cancellationToken);

        // 5. Update streak calculation
        var streakResult = await _mediator.Send(new UpdateStreakCommand
        {
            UserId = request.UserId,
            CompletedAt = request.CompletedAt
        }, cancellationToken);

        // 6. Evaluate achievements
        var achievementsResult = await _mediator.Send(new EvaluateAchievementsOrchestrator(
            request.UserId,
            request.CompletedAt), cancellationToken);

        var newAchievements = achievementsResult.IsSuccess ? achievementsResult.Value : new List<EarnedAchievementDto>();

        // 7. Stage WorkoutLoggedEvent in outbox
        await _publishEndpoint.Publish(new WorkoutLoggedEvent
        {
            UserId = request.UserId,
            LogId = saveResult.Value,
            WorkoutId = request.WorkoutId,
            CaloriesBurned = request.CaloriesBurned,
            DurationInMinutes = request.Duration,
            CompletedAt = request.CompletedAt
        }, cancellationToken);

        // 8. Post-commit cache invalidation
        await _cacheInvalidator.InvalidateUserAsync(request.UserId, cancellationToken);

        var currentStreak = streakResult.IsSuccess ? streakResult.Value.CurrentStreak : 0;
        var streakUpdated = streakResult.IsSuccess && streakResult.Value.StreakUpdated;

        _logger.LogInformation("Workout logged successfully for user {UserId}. LogId: {LogId}, CurrentStreak: {Streak}",
            request.UserId, saveResult.Value, currentStreak);

        return Result.Success(new LogWorkoutResponse(
            saveResult.Value,
            streakUpdated,
            currentStreak,
            newAchievements));
    }
}