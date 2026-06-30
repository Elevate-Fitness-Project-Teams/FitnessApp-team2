using AuthenticationService.Common;
using AuthenticationService.Data;
using AuthenticationService.Features.Auth.Commands.UpdateUserPassword;
using AuthenticationService.Features.Auth.Commands.VerifyOtp;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.ResetPassword;

public class ResetPasswordOrchestratorHandler : IRequestHandler<ResetPasswordOrchestrator, Result<bool>>
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public ResetPasswordOrchestratorHandler(IMediator mediator, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(ResetPasswordOrchestrator request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            var verifyOtpResult =
                await _mediator.Send(new VerifyOtpCommand(request.Email, request.Otp), cancellationToken);
            if (verifyOtpResult.IsFailure) return Result<bool>.Failure(verifyOtpResult.Errors);

            var updatePasswordResult =
                await _mediator.Send(new UpdateUserPasswordCommand(request.Email, request.NewPassword),
                    cancellationToken);
            if (updatePasswordResult.IsFailure) return Result<bool>.Failure(updatePasswordResult.Errors);

            return Result<bool>.Success(true);
        }, cancellationToken);
    }
}