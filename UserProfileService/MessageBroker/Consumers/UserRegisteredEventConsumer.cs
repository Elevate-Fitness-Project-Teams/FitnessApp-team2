using MassTransit;
using MediatR;
using MessageBroker.Events;
using UserProfileService.Features.Profiles.CreateDefaultProfile;

namespace UserProfileService.MessageBroker.Consumers;

public class UserRegisteredEventConsumer : IConsumer<UserRegisteredIntegrationEvent>
{
    private readonly IMediator _mediator;

    public UserRegisteredEventConsumer(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<UserRegisteredIntegrationEvent> context)
    {
        var message = context.Message;
        var command = new CreateDefaultProfileCommand(
            message.UserId,
            message.FirstName,
            message.LastName,
            message.Email,
            message.PhoneNumber,
            message.RegisteredAt
        );
        var result = await _mediator.Send(command, context.CancellationToken);
        if (!result.IsSuccess)
        {
            throw new Exception($"Failed to create profile for user {message.UserId}: {result.Error}");
        }

    }
}
