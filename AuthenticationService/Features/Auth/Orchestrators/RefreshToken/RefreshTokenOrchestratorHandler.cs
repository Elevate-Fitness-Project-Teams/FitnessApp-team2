using AuthenticationService.Common;
using AuthenticationService.Data;
using AuthenticationService.Features.Auth.Commands.CreateRefreshToken;
using AuthenticationService.Features.Auth.Commands.RevokeRefreshToken;
using AuthenticationService.Features.Auth.Queries.GetRefreshToken;
using AuthenticationService.Features.Auth.Queries.GetUserById;
using AuthenticationService.Services;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.RefreshToken;

public class
    RefreshTokenOrchestratorHandler : IRequestHandler<RefreshTokenOrchestrator,
    Result<RefreshTokenOrchestratorResponse>>
{
    private readonly IJwtProvider _jwtProvider;
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenOrchestratorHandler(
        IMediator mediator,
        IJwtProvider jwtProvider,
        IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _jwtProvider = jwtProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RefreshTokenOrchestratorResponse>> Handle(RefreshTokenOrchestrator request,
        CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            var tokenResult = await _mediator.Send(new GetRefreshTokenQuery(request.RefreshToken), cancellationToken);

            if (tokenResult.IsFailure || tokenResult.Value.ExpiresAt < DateTime.UtcNow ||
                tokenResult.Value.RevokedAt != null)
                return Result<RefreshTokenOrchestratorResponse>.Failure(Error.Failure("AUTH_INVALID_REFRESH_TOKEN",
                    "Invalid, expired, or revoked refresh token."));

            var userResult = await _mediator.Send(new GetUserByIdQuery(tokenResult.Value.UserId), cancellationToken);
            if (userResult.IsFailure ||
                (userResult.Value.IsLockedOut && userResult.Value.LockedUntil > DateTime.UtcNow))
                return Result<RefreshTokenOrchestratorResponse>.Failure(Error.Failure("AUTH_USER_UNAUTHORIZED",
                    "User is locked or not found."));

            await _mediator.Send(new RevokeRefreshTokenCommand(request.RefreshToken), cancellationToken);

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