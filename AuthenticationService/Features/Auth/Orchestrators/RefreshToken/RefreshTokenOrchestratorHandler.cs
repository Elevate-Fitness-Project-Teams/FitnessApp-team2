using AuthenticationService.Common;
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
    private readonly IMediator _mediator;
    private readonly IJwtProvider _jwtProvider;

    public RefreshTokenOrchestratorHandler(
        IMediator mediator, 
        IJwtProvider jwtProvider)
    {
        _mediator = mediator;
        _jwtProvider = jwtProvider;
    }

    public async Task<Result<RefreshTokenOrchestratorResponse>> Handle(RefreshTokenOrchestrator request,
        CancellationToken cancellationToken)
    {
        var tokenEntity = await _mediator.Send(new GetRefreshTokenQuery(request.RefreshToken), cancellationToken);

        if (tokenEntity == null || tokenEntity.ExpiresAt < DateTime.UtcNow || tokenEntity.RevokedAt != null)
            return Result<RefreshTokenOrchestratorResponse>.Failure(Error.Failure("AUTH_INVALID_REFRESH_TOKEN",
                "Invalid, expired, or revoked refresh token."));

        var user = await _mediator.Send(new GetUserByIdQuery(tokenEntity.UserId), cancellationToken);
        if (user == null || (user.IsLockedOut && user.LockedUntil > DateTime.UtcNow))
            return Result<RefreshTokenOrchestratorResponse>.Failure(Error.Failure("AUTH_USER_UNAUTHORIZED",
                "User is locked or not found."));

        await _mediator.Send(new RevokeRefreshTokenCommand(request.RefreshToken), cancellationToken);

        var (accessToken, expiresIn) = _jwtProvider.GenerateToken(user);
        var newRefreshToken = _jwtProvider.GenerateRefreshToken();

        await _mediator.Send(
            new CreateRefreshTokenCommand(user.Id, newRefreshToken, DateTime.UtcNow.AddDays(7)),
            cancellationToken);

        return Result<RefreshTokenOrchestratorResponse>.Success(
            new RefreshTokenOrchestratorResponse(accessToken, newRefreshToken));
    }
}