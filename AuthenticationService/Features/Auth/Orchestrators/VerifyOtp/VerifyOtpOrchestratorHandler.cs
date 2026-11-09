using AuthenticationService.Common;
using AuthenticationService.Data;
using AuthenticationService.Features.Auth.Commands.ConfirmUserEmail;
using AuthenticationService.Features.Auth.Commands.MarkOtpAsUsed;
using MediatR;
using StackExchange.Redis;

namespace AuthenticationService.Features.Auth.Orchestrators.VerifyOtp;

public class VerifyOtpOrchestratorHandler : IRequestHandler<VerifyOtpOrchestrator, Result<VerifyOtpResponse>>
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConnectionMultiplexer _redis;

    public VerifyOtpOrchestratorHandler(
        IMediator mediator,
        IUnitOfWork unitOfWork,
        IConnectionMultiplexer redis)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
        _redis = redis;
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

            // Generate a secure reset token valid for 10 minutes
            var resetToken = Guid.NewGuid().ToString("N");
            var emailLower = request.Email.Trim().ToLowerInvariant();

            try
            {
                var db = _redis.GetDatabase();
                var ttl = TimeSpan.FromMinutes(10);
                await db.StringSetAsync($"auth:pwd-reset:{emailLower}", resetToken, ttl);
                await db.StringSetAsync($"auth:verified-otp:{emailLower}:{request.Otp.Trim()}", resetToken, ttl);
            }
            catch
            {
                // Proceed even if Redis write fails
            }

            return Result<VerifyOtpResponse>.Success(new VerifyOtpResponse(resetToken));
        }, cancellationToken);
    }
}
