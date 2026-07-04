using AuthenticationService.Common;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.Login;

public record LoginOrchestrator(
    string Email,
    string Password) : IRequest<Result<LoginResponse>>;

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    bool ProfileCompleted,
    bool IsPremium);