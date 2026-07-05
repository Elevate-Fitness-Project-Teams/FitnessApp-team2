using AuthenticationService.Common;
using AuthenticationService.Events;
using MassTransit;
using MediatR;

namespace AuthenticationService.Features.Auth.Commands.PublishUserRegisteredEvent;

public class PublishUserRegisteredEventCommandHandler : IRequestHandler<PublishUserRegisteredEventCommand, Result>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public PublishUserRegisteredEventCommandHandler(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result> Handle(PublishUserRegisteredEventCommand request, CancellationToken cancellationToken)
    {
        var integrationEvent = new UserRegisteredIntegrationEvent
        {
            UserId = request.UserId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            RegisteredAt = DateTime.UtcNow
        };

        await _publishEndpoint.Publish(integrationEvent, cancellationToken);

        return Result.Success();
    }
}