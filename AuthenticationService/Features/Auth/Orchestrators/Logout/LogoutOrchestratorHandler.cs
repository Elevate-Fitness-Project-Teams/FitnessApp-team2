using AuthenticationService.Common;
using AuthenticationService.Data;
using AuthenticationService.Features.Auth.Commands.RevokeRefreshToken;
using AuthenticationService.Services;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.Logout;

public class LogoutOrchestratorHandler : IRequestHandler<LogoutOrchestrator, Result>
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenRevocationStore _tokenRevocationStore;

    public LogoutOrchestratorHandler(
        IMediator mediator,
        IUnitOfWork unitOfWork,
        ITokenRevocationStore tokenRevocationStore)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
        _tokenRevocationStore = tokenRevocationStore;
    }

    public async Task<Result> Handle(LogoutOrchestrator request, CancellationToken cancellationToken)
    {
        // 1. Revoke the Refresh Token in SQL Server (ensuring it belongs to this user)
        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            await _mediator.Send(new RevokeRefreshTokenCommand(request.RefreshToken, request.UserId), cancellationToken);
        }

        // 2. Blacklist the current JWT Access Token in Redis by JTI for its remaining lifetime
        if (!string.IsNullOrWhiteSpace(request.Jti))
        {
            var expiresAt = request.ExpiresAtUtc ?? DateTime.UtcNow.AddMinutes(15);
            var ttl = expiresAt - DateTime.UtcNow;

            if (ttl > TimeSpan.Zero)
            {
                await _tokenRevocationStore.RevokeTokenAsync(request.Jti, ttl, cancellationToken);
            }
        }

        return Result.Success();
    }
}
