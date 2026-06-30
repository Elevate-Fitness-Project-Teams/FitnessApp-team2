using MediatR;

namespace AuthenticationService.Features.Auth.Commands.CreateRefreshToken;

public record CreateRefreshTokenCommand(string UserId, string Token, DateTime ExpiresAt) : IRequest;