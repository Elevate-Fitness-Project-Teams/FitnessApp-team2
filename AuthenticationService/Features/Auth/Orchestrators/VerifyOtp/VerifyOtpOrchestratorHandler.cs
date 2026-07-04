using AuthenticationService.Common;
using AuthenticationService.Features.Auth.Commands.ConfirmUserEmail;
using AuthenticationService.Features.Auth.Commands.MarkOtpAsUsed;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.VerifyOtp;

public class VerifyOtpOrchestratorHandler : IRequestHandler<VerifyOtpOrchestrator, Result<VerifyOtpResponse>>
{
    private readonly IMediator _mediator;

    public VerifyOtpOrchestratorHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Result<VerifyOtpResponse>> Handle(VerifyOtpOrchestrator request,
        CancellationToken cancellationToken)
    {
        var markOtpResult = await _mediator.Send(new MarkOtpAsUsedCommand(request.Email, request.Otp), cancellationToken);

        if (markOtpResult.IsFailure)
        {
            return Result<VerifyOtpResponse>.Failure(markOtpResult.Errors);
        }

        var confirmEmailResult = await _mediator.Send(new ConfirmUserEmailCommand(request.Email), cancellationToken);

        if (confirmEmailResult.IsFailure)
        {
            return Result<VerifyOtpResponse>.Failure(confirmEmailResult.Errors);
        }

        // Generate a short-lived reset token
        var resetToken = Guid.NewGuid().ToString("N");

        // TODO: Store resetToken with a short expiry

        return Result<VerifyOtpResponse>.Success(new VerifyOtpResponse(resetToken));
    }
}