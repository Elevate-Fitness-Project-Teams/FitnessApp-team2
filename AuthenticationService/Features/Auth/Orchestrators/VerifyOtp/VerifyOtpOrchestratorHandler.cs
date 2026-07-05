using AuthenticationService.Common;
using AuthenticationService.Data;
using AuthenticationService.Features.Auth.Commands.ConfirmUserEmail;
using AuthenticationService.Features.Auth.Commands.MarkOtpAsUsed;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.VerifyOtp;

public class VerifyOtpOrchestratorHandler : IRequestHandler<VerifyOtpOrchestrator, Result<VerifyOtpResponse>>
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public VerifyOtpOrchestratorHandler(IMediator mediator, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<VerifyOtpResponse>> Handle(VerifyOtpOrchestrator request,
        CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            var markOtpResult =
                await _mediator.Send(new MarkOtpAsUsedCommand(request.Email, request.Otp), cancellationToken);

            if (markOtpResult.IsFailure) return Result<VerifyOtpResponse>.Failure(markOtpResult.Errors);

            var confirmEmailResult =
                await _mediator.Send(new ConfirmUserEmailCommand(request.Email), cancellationToken);

            if (confirmEmailResult.IsFailure) return Result<VerifyOtpResponse>.Failure(confirmEmailResult.Errors);

            // Generate a short-lived reset token
            var resetToken = Guid.NewGuid().ToString("N");

            // TODO: Store resetToken with a short expiry

            return Result<VerifyOtpResponse>.Success(new VerifyOtpResponse(resetToken));
        }, cancellationToken);
    }
}