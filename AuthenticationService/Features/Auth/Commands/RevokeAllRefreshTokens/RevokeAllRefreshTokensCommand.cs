using AuthenticationService.Common;
using MediatR;

namespace AuthenticationService.Features.Auth.Commands.RevokeAllRefreshTokens;

public record RevokeAllRefreshTokensCommand(string UserId) : IRequest<Result>;
