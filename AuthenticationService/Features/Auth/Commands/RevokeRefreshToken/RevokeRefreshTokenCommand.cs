using MediatR;

namespace AuthenticationService.Features.Auth.Commands.RevokeRefreshToken;

public record RevokeRefreshTokenCommand(string Token) : IRequest<Unit>;