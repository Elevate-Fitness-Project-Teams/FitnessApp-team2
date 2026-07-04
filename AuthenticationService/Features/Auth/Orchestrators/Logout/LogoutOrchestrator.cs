using AuthenticationService.Common;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.Logout;

public record LogoutOrchestrator(string RefreshToken, string UserId) : IRequest<Result<Unit>>;
