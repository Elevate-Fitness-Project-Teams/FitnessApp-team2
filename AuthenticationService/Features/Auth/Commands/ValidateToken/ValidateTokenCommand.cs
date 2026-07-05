using AuthenticationService.Common;
using MediatR;

namespace AuthenticationService.Features.Auth.Commands.ValidateToken;

public record ValidateTokenCommand(string Token) : IRequest<Result>;
