using AuthenticationService.Common;
using MediatR;

namespace AuthenticationService.Features.Auth.Commands.RevokeRefreshToken;

public record RevokeRefreshTokenCommand(string Token) : IRequest<Result>;