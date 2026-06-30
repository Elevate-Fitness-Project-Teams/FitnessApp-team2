using AuthenticationService.Common;
using AuthenticationService.Features.Auth.Commands.SendOtp;
using AuthenticationService.Features.Auth.Queries.GetUserByEmail;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.ForgotPassword;

public class ForgotPasswordOrchestratorHandler : IRequestHandler<ForgotPasswordOrchestrator, Result<bool>>
{
    private readonly IMediator _mediator;

    public ForgotPasswordOrchestratorHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Result<bool>> Handle(ForgotPasswordOrchestrator request, CancellationToken cancellationToken)
    {
        // 1. Check if user exists
        var user = await _mediator.Send(new GetUserByEmailQuery(request.Email), cancellationToken);
        if (user == null) return Result<bool>.Failure(Error.NotFound(AuthErrorCodes.UserNotFound, "User not found"));

        // 2. Generate and Send OTP
        var sendOtpResult = await _mediator.Send(new SendOtpCommand(request.Email), cancellationToken);
        if (sendOtpResult.IsFailure) return Result<bool>.Failure(sendOtpResult.Errors);

        return Result<bool>.Success(true);
    }
}