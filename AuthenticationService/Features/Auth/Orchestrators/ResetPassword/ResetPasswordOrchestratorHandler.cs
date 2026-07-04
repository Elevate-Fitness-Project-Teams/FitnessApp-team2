using AuthenticationService.Common;
using AuthenticationService.Features.Auth.Commands.UpdateUserPassword;
using AuthenticationService.Features.Auth.Orchestrators.VerifyOtp;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.ResetPassword;

public class ResetPasswordOrchestratorHandler : IRequestHandler<ResetPasswordOrchestrator, Result<bool>>
{
    private readonly IMediator _mediator;

    public ResetPasswordOrchestratorHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Result<bool>> Handle(ResetPasswordOrchestrator request, CancellationToken cancellationToken)
    {
        var verifyOtpResult =
            await _mediator.Send(new VerifyOtpOrchestrator(request.Email, request.Otp), cancellationToken);
        if (verifyOtpResult.IsFailure) return Result<bool>.Failure(verifyOtpResult.Errors);

        var updatePasswordResult =
            await _mediator.Send(new UpdateUserPasswordCommand(request.Email, request.NewPassword),
                cancellationToken);
        if (updatePasswordResult.IsFailure) return Result<bool>.Failure(updatePasswordResult.Errors);

        return Result<bool>.Success(true);
    }
}