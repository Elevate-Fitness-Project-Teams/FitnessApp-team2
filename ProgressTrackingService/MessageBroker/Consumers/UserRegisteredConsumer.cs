using MassTransit;
using MediatR;
using ProgressTrackingService.Features.Users.Commands.InitializeUserProgress;

namespace ProgressTrackingService.MessageBroker.Consumers;

public class UserRegisteredEvent
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<UserRegisteredConsumer> _logger;

    public UserRegisteredConsumer(
        IMediator mediator,
        ILogger<UserRegisteredConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var userId = context.Message.UserId;
        _logger.LogInformation("Processing UserRegisteredEvent for User {UserId}", userId);

        var result = await _mediator.Send(new InitializeUserProgressCommand(userId), context.CancellationToken);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Successfully initialized baseline progress records for User {UserId}", userId);
        }
        else
        {
            _logger.LogError("Failed to initialize progress records for User {UserId}: {Error}", userId, result.Error?.Description);
            throw new InvalidOperationException($"Failed to initialize progress for user {userId}: {result.Error?.Description}");
        }
    }
}
