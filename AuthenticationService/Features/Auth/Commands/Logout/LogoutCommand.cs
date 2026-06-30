using AuthenticationService.Common;
using MediatR;

namespace AuthenticationService.Features.Auth.Commands.Logout;

public record LogoutCommand(string RefreshToken, string UserId) : IRequest<Result<Unit>>;