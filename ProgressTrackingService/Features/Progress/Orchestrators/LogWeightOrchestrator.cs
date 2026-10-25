using MassTransit;
using MediatR;
using MessageBroker.Events;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Caching;
using ProgressTrackingService.Common.Domain;
using ProgressTrackingService.Features.Achievements.Orchestrators.EvaluateAchievements;
using ProgressTrackingService.Features.Biometrics.Queries.GetUserHeight;
using ProgressTrackingService.Features.Progress.Commands.LogWeight;
using ProgressTrackingService.Features.Progress.Queries.GetWeightBeforeDate;
using ProgressTrackingService.Features.Progress.Queries.GetWeightBounds;

namespace ProgressTrackingService.Features.Progress.Orchestrators;

public record LogWeightOrchestrator(Guid UserId, double Weight, DateTime Date, string? Notes)
    : IRequest<Result<LogWeightResponse>>;

public class LogWeightOrchestratorHandler : IRequestHandler<LogWeightOrchestrator, Result<LogWeightResponse>>
{
    private readonly IMediator _mediator;
    private readonly ICacheInvalidator _cacheInvalidator;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<LogWeightOrchestratorHandler> _logger;

    public LogWeightOrchestratorHandler(
        IMediator mediator,
        ICacheInvalidator cacheInvalidator,
        IPublishEndpoint publishEndpoint,
        ILogger<LogWeightOrchestratorHandler> logger)
    {
        _mediator = mediator;
        _cacheInvalidator = cacheInvalidator;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<Result<LogWeightResponse>> Handle(LogWeightOrchestrator request, CancellationToken cancellationToken)
    {
        // 1. Get previous weight chronologically before this entry's date
        var prevWeightResult = await _mediator.Send(
            new GetWeightBeforeDateQuery(request.UserId, request.Date),
            cancellationToken);

        var previousWeight = prevWeightResult.Value;
        var diffFromPrevious = previousWeight.HasValue
            ? Math.Round(request.Weight - previousWeight.Value, 2)
            : 0.0;

        // 2. Save weight entry
        var saveResult = await _mediator.Send(new SaveWeightEntryCommand
        {
            UserId = request.UserId,
            Weight = request.Weight,
            Date = request.Date,
            Notes = request.Notes
        }, cancellationToken);

        if (!saveResult.IsSuccess)
        {
            return Result.Failure<LogWeightResponse>(saveResult.Error);
        }

        // 3. Compute weight bounds (initial, latest, isLatest)
        var boundsResult = await _mediator.Send(
            new GetWeightBoundsQuery(request.UserId, request.Date),
            cancellationToken);

        var bounds = boundsResult.Value;

        // 4. Update user statistics
        await _mediator.Send(
            new UpdateWeightStatisticCommand(request.UserId, bounds.InitialWeight, bounds.LatestWeight),
            cancellationToken);

        // 5. Evaluate achievements (e.g. WEIGHT_LOST_1, WEIGHT_LOST_5)
        await _mediator.Send(
            new EvaluateAchievementsOrchestrator(request.UserId, request.Date),
            cancellationToken);

        // 6. If this entry is chronologically the latest, publish WeightUpdatedEvent to FCE
        if (bounds.IsNewEntryLatest)
        {
            await _publishEndpoint.Publish(new WeightUpdatedEvent
            {
                UserId = request.UserId,
                NewWeight = request.Weight,
                RecordedAt = request.Date
            }, cancellationToken);
        }

        // 7. Get user height to derive BMI
        var heightResult = await _mediator.Send(new GetUserHeightQuery(request.UserId), cancellationToken);
        var bmi = BmiCalculator.Calculate(request.Weight, heightResult.Value);

        var totalWeightLost = Math.Max(0.0, Math.Round(bounds.InitialWeight - bounds.LatestWeight, 2));

        // 8. Post-commit cache invalidation
        await _cacheInvalidator.InvalidateUserAsync(request.UserId, cancellationToken);

        var response = new LogWeightResponse(
            saveResult.Value,
            request.Weight,
            bmi,
            diffFromPrevious,
            totalWeightLost);

        _logger.LogInformation("Weight logged successfully for user {UserId}: {Weight} kg, diff={Diff}, lost={Lost}",
            request.UserId, request.Weight, diffFromPrevious, totalWeightLost);

        return Result.Success(response);
    }
}
