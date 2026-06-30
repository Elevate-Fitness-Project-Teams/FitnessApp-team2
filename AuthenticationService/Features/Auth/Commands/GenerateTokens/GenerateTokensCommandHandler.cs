using AuthenticationService.Services;
using MediatR;

namespace AuthenticationService.Features.Auth.Commands.GenerateTokens;

public class GenerateTokensCommandHandler : IRequestHandler<GenerateTokensCommand, (string AccessToken, string
    RefreshToken, int ExpiresIn)>
{
    private readonly IJwtProvider _jwtProvider;

    public GenerateTokensCommandHandler(IJwtProvider jwtProvider)
    {
        _jwtProvider = jwtProvider;
    }

    public Task<(string AccessToken, string RefreshToken, int ExpiresIn)> Handle(GenerateTokensCommand request,
        CancellationToken cancellationToken)
    {
        var (accessToken, expiresIn) = _jwtProvider.GenerateToken(request.User);
        var refreshToken = _jwtProvider.GenerateRefreshToken();
        return Task.FromResult((accessToken, refreshToken, expiresIn));
    }
}