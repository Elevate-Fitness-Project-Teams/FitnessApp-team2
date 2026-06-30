using AuthenticationService.Data.Entities;
using MediatR;

namespace AuthenticationService.Features.Auth.Commands.GenerateTokens;

public record GenerateTokensCommand(User User) : IRequest<(string AccessToken, string RefreshToken, int ExpiresIn)>;