using AuthenticationService.Common;
using MediatR;

namespace AuthenticationService.Features.Auth.Commands.PublishUserRegisteredEvent;

public record PublishUserRegisteredEventCommand(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber) : IRequest<Result>;