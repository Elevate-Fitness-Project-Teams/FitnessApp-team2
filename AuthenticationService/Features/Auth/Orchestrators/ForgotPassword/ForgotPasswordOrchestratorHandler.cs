using AuthenticationService.Common;
using AuthenticationService.Features.Auth.Queries.GetUserByEmail;
using AuthenticationService.Data;
using AuthenticationService.Features.Auth.Orchestrators.SendOtp;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.ForgotPassword;

public class ForgotPasswordOrchestratorHandler : IRequestHandler<ForgotPasswordOrchestrator, Result<bool>>
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public ForgotPasswordOrchestratorHandler(IMediator mediator, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(ForgotPasswordOrchestrator request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            // 1. Check if user exists
            var user = await _mediator.Send(new GetUserByEmailQuery(request.Email), cancellationToken);
            if (user == null) return Result<bool>.Success(true);

            // 2. Generate and Send OTP
            var sendOtpResult = await _mediator.Send(new SendOtpOrchestrator(request.Email), cancellationToken);
            if (sendOtpResult.IsFailure) return Result<bool>.Failure(sendOtpResult.Errors);

            return Result<bool>.Success(true);
        }, cancellationToken);
    }
}