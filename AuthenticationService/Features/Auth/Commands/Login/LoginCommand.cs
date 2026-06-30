using AuthenticationService.Common;
using MediatR;

namespace AuthenticationService.Features.Auth.Commands.Login;

public class LoginCommand : IRequest<Result<LoginCommandResponse>>
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}