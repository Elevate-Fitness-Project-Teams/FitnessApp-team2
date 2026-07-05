using AuthenticationService.Common;
using AuthenticationService.Data;
using AuthenticationService.Features.Auth.Commands.RevokeRefreshToken;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.Logout;

public class LogoutOrchestratorHandler : IRequestHandler<LogoutOrchestrator, Result<Unit>>
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutOrchestratorHandler(IMediator mediator, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(LogoutOrchestrator request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            // For logout, we just revoke the refresh token. The JWT token will naturally expire.
            var result = await _mediator.Send(new RevokeRefreshTokenCommand(request.RefreshToken), cancellationToken);

            if (result.IsFailure) return Result<Unit>.Failure(result.Errors);

            return Result<Unit>.Success(Unit.Value);
        }, cancellationToken);
    }
}