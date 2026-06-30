using AuthenticationService.Common;
using MediatR;

namespace AuthenticationService.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string PhoneNumber) : IRequest<Result<RegisterCommandResponse>>;

public record RegisterCommandResponse(
    string UserId,
    bool RequiresProfileCompletion);