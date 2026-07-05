using AuthenticationService.Common;
using AuthenticationService.Data;
using AuthenticationService.Features.Auth.Commands.ChangeUserPassword;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.ChangePassword;

public class ChangePasswordOrchestratorHandler : IRequestHandler<ChangePasswordOrchestrator, Result<bool>>
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public ChangePasswordOrchestratorHandler(IMediator mediator, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(ChangePasswordOrchestrator request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            var result =
                await _mediator.Send(
                    new ChangeUserPasswordCommand(request.UserId, request.OldPassword, request.NewPassword),
                    cancellationToken);

            if (result.IsFailure) return Result<bool>.Failure(result.Errors);

            return Result<bool>.Success(true);
        }, cancellationToken);
    }
}