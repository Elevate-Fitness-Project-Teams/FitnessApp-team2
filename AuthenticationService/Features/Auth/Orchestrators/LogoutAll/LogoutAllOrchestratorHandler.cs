using AuthenticationService.Common;
using AuthenticationService.Features.Auth.Commands.RevokeAllRefreshTokens;
using AuthenticationService.Services;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.LogoutAll;

public class LogoutAllOrchestratorHandler : IRequestHandler<LogoutAllOrchestrator, Result>
{
    private readonly IMediator _mediator;
    private readonly ITokenRevocationStore _tokenRevocationStore;

    public LogoutAllOrchestratorHandler(IMediator mediator, ITokenRevocationStore tokenRevocationStore)
    {
        _mediator = mediator;
        _tokenRevocationStore = tokenRevocationStore;
    }

    public async Task<Result> Handle(LogoutAllOrchestrator request, CancellationToken cancellationToken)
    {
        // 1. Revoke all active Refresh Tokens for this user in SQL Server
        await _mediator.Send(new RevokeAllRefreshTokensCommand(request.UserId), cancellationToken);

        // 2. Set user revocation cutoff in Redis (all tokens issued at or before now are rejected)
        if (Guid.TryParse(request.UserId, out var userGuid))
        {
            await _tokenRevocationStore.RevokeAllUserTokensAsync(
                userGuid,
                DateTimeOffset.UtcNow,
                TimeSpan.FromMinutes(15),
                cancellationToken);
        }

        return Result.Success();
    }
}

