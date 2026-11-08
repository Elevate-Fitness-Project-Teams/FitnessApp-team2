using AuthenticationService.Common;
using AuthenticationService.Data;
using AuthenticationService.Features.Auth.Commands.CreateRefreshToken;
using AuthenticationService.Features.Auth.Commands.RevokeAllRefreshTokens;
using AuthenticationService.Features.Auth.Commands.RevokeRefreshToken;
using AuthenticationService.Features.Auth.Queries.GetRefreshToken;
using AuthenticationService.Features.Auth.Queries.GetUserById;
using AuthenticationService.Services;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.RefreshToken;

public class RefreshTokenOrchestratorHandler : IRequestHandler<RefreshTokenOrchestrator, Result<RefreshTokenOrchestratorResponse>>
{
    private readonly IJwtProvider _jwtProvider;
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenRevocationStore _tokenRevocationStore;

    public RefreshTokenOrchestratorHandler(
        IMediator mediator,
        IJwtProvider jwtProvider,
        IUnitOfWork unitOfWork,
        ITokenRevocationStore tokenRevocationStore)
    {
        _mediator = mediator;
        _jwtProvider = jwtProvider;
        _unitOfWork = unitOfWork;
        _tokenRevocationStore = tokenRevocationStore;
    }

    public async Task<Result<RefreshTokenOrchestratorResponse>> Handle(RefreshTokenOrchestrator request,
        CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            var tokenResult = await _mediator.Send(new GetRefreshTokenQuery(request.RefreshToken), cancellationToken);

            if (tokenResult.IsFailure)
            {
                return Result<RefreshTokenOrchestratorResponse>.Failure(Error.Failure(
                    "AUTH_INVALID_REFRESH_TOKEN",
                    "Invalid refresh token."));
            }

            // REFRESH TOKEN REUSE / THEFT DETECTION (RFC 6819)
            // If the presented token has already been revoked, someone is replaying an old refresh token.
            // Invalidate all tokens for this user immediately to protect the account.
            if (tokenResult.Value.RevokedAt != null)
            {
                await _mediator.Send(new RevokeAllRefreshTokensCommand(tokenResult.Value.UserId), cancellationToken);

                if (Guid.TryParse(tokenResult.Value.UserId, out var victimId))
                {
                    await _tokenRevocationStore.RevokeAllUserTokensAsync(
                        victimId,
                        DateTimeOffset.UtcNow,
                        TimeSpan.FromMinutes(15),
                        cancellationToken);
                }

                return Result<RefreshTokenOrchestratorResponse>.Failure(Error.Unauthorized(
                    "AUTH_TOKEN_THEFT_DETECTED",
                    "Refresh token reuse detected. All active sessions have been revoked for your security."));
            }

            if (tokenResult.Value.ExpiresAt < DateTime.UtcNow)
            {
                return Result<RefreshTokenOrchestratorResponse>.Failure(Error.Failure(
                    "AUTH_INVALID_REFRESH_TOKEN",
                    "Refresh token has expired."));
            }

            var userResult = await _mediator.Send(new GetUserByIdQuery(tokenResult.Value.UserId), cancellationToken);
            if (userResult.IsFailure ||
                (userResult.Value.IsLockedOut && userResult.Value.LockedUntil > DateTime.UtcNow))
            {
                return Result<RefreshTokenOrchestratorResponse>.Failure(Error.Failure(
                    "AUTH_USER_UNAUTHORIZED",
                    "User is locked or not found."));
            }

            // Revoke the old refresh token (ensuring it belongs to this user)
            await _mediator.Send(new RevokeRefreshTokenCommand(request.RefreshToken, userResult.Value.Id), cancellationToken);

            // Generate new token pair
            var (accessToken, expiresIn) = _jwtProvider.GenerateToken(userResult.Value);
            var newRefreshToken = _jwtProvider.GenerateRefreshToken();

            await _mediator.Send(
                new CreateRefreshTokenCommand(userResult.Value.Id, newRefreshToken, DateTime.UtcNow.AddDays(7)),
                cancellationToken);

            return Result<RefreshTokenOrchestratorResponse>.Success(
                new RefreshTokenOrchestratorResponse(accessToken, newRefreshToken));
        }, cancellationToken);
    }
}
