using MassTransit;
using MediatR;
using MessageBroker.Events;
using ProgressTrackingService.Features.Biometrics.Commands.UpsertUserBiometrics;

namespace ProgressTrackingService.MessageBroker.Consumers;

public class FitnessStatsSubmittedConsumer : IConsumer<FitnessStatsSubmittedEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<FitnessStatsSubmittedConsumer> _logger;

    public FitnessStatsSubmittedConsumer(IMediator mediator, ILogger<FitnessStatsSubmittedConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<FitnessStatsSubmittedEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Received FitnessStatsSubmittedEvent for User {UserId}: Height={Height}cm, Weight={Weight}kg",
            msg.UserId, msg.HeightCm, msg.WeightKg);

        var command = new UpsertUserBiometricsCommand(msg.UserId, msg.HeightCm, msg.RecordedAt);
        var result = await _mediator.Send(command, context.CancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogError("Failed to project biometrics for user {UserId}: {Error}",
                msg.UserId, result.Error.Description);
            throw new InvalidOperationException($"Biometrics projection failed: {result.Error.Description}");
        }
    }
}
