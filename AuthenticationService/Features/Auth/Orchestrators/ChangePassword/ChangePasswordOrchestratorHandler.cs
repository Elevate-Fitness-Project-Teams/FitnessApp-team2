using AuthenticationService.Common;
using AuthenticationService.Features.Auth.Commands.ChangeUserPassword;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.ChangePassword;

public class ChangePasswordOrchestratorHandler : IRequestHandler<ChangePasswordOrchestrator, Result<bool>>
{
    private readonly IMediator _mediator;

    public ChangePasswordOrchestratorHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Result<bool>> Handle(ChangePasswordOrchestrator request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ChangeUserPasswordCommand(request.UserId, request.OldPassword, request.NewPassword), cancellationToken);

        if (result.IsFailure) return Result<bool>.Failure(result.Errors);

        return Result<bool>.Success(true);
    }
}