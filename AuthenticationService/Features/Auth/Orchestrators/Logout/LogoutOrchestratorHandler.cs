using AuthenticationService.Common;
using AuthenticationService.Features.Auth.Commands.RevokeRefreshToken;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.Logout;

public class LogoutOrchestratorHandler : IRequestHandler<LogoutOrchestrator, Result<Unit>>
{
    private readonly IMediator _mediator;

    public LogoutOrchestratorHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Result<Unit>> Handle(LogoutOrchestrator request, CancellationToken cancellationToken)
    {
        // For logout, we just revoke the refresh token. The JWT token will naturally expire.
        var result = await _mediator.Send(new RevokeRefreshTokenCommand(request.RefreshToken), cancellationToken);

        if (result.IsFailure)
        {
            return Result<Unit>.Failure(result.Errors);
        }

        return Result<Unit>.Success(Unit.Value);
    }
}
