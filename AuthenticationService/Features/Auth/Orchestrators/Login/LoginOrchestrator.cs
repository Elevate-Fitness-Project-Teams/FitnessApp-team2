using AuthenticationService.Common;
using AuthenticationService.Features.Auth.Commands.Login;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.Login;

public record LoginOrchestrator(
    string Email,
    string Password) : IRequest<Result<LoginCommandResponse>>;