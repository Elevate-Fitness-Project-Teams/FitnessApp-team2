using FitnessCalculationService.Features.Calculations.Commands.RecalculateMetrics;
using MassTransit;
using MediatR;
using MessageBroker.Events;

namespace FitnessCalculationService.MessageBroker.Consumers;

public class WeightUpdatedConsumer : IConsumer<WeightUpdatedEvent>
{
    private readonly ILogger<WeightUpdatedConsumer> _logger;
    private readonly IMediator _mediator;

    public WeightUpdatedConsumer(IMediator mediator, ILogger<WeightUpdatedConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<WeightUpdatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "Received WeightUpdatedEvent for User {UserId} with NewWeight {NewWeight} recorded at {RecordedAt}",
            message.UserId, message.NewWeight, message.RecordedAt);

        var orchestrator = new RecalculateMetricsOrchestrator(
            message.UserId,
            message.NewWeight,
            "ProgressTrackingService weight update event",
            "progress_service",
            message.RecordedAt,
            context.MessageId
        );

        var result = await _mediator.Send(orchestrator, context.CancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning(
                "Recalculation via WeightUpdatedEvent failed for User {UserId}: {Code} - {Message}",
                message.UserId, result.Error?.Code, result.Error?.Message);
        }
        else
        {
            _logger.LogInformation(
                "Recalculation via WeightUpdatedEvent succeeded for User {UserId}. PlanReassigned: {Reassigned}, NewStatus: {Status}",
                message.UserId, result.Value?.PlanReassigned, result.Value?.NewStatus);
        }
    }
}
