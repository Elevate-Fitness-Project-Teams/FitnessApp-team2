using AuthenticationService.Common;
using AuthenticationService.Data;
using AuthenticationService.Features.Auth.Commands.CreateRefreshToken;
using AuthenticationService.Features.Auth.Commands.GenerateTokens;
using AuthenticationService.Features.Auth.Commands.RevokeRefreshToken;
using AuthenticationService.Features.Auth.Queries.GetRefreshToken;
using AuthenticationService.Features.Auth.Queries.GetUserById;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.RefreshToken;

public class
    RefreshTokenOrchestratorHandler : IRequestHandler<RefreshTokenOrchestrator,
    Result<RefreshTokenOrchestratorResponse>>
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenOrchestratorHandler(IMediator mediator, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RefreshTokenOrchestratorResponse>> Handle(RefreshTokenOrchestrator request,
        CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
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

            var tokens = await _mediator.Send(new GenerateTokensCommand(user), cancellationToken);

            await _mediator.Send(
                new CreateRefreshTokenCommand(user.Id, tokens.RefreshToken, DateTime.UtcNow.AddDays(7)),
                cancellationToken);

            return Result<RefreshTokenOrchestratorResponse>.Success(
                new RefreshTokenOrchestratorResponse(tokens.AccessToken, tokens.RefreshToken));
        }, cancellationToken);
    }
}