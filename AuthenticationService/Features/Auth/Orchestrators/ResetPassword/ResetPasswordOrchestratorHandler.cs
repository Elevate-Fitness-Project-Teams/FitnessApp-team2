using AuthenticationService.Common;
using AuthenticationService.Data;
using AuthenticationService.Features.Auth.Commands.RevokeAllRefreshTokens;
using AuthenticationService.Features.Auth.Commands.UpdateUserPassword;
using AuthenticationService.Features.Auth.Orchestrators.VerifyOtp;
using AuthenticationService.Features.Auth.Queries.GetUserByEmail;
using AuthenticationService.Services;
using MediatR;
using StackExchange.Redis;

namespace AuthenticationService.Features.Auth.Orchestrators.ResetPassword;

public class ResetPasswordOrchestratorHandler : IRequestHandler<ResetPasswordOrchestrator, Result<bool>>
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConnectionMultiplexer _redis;
    private readonly ITokenRevocationStore _tokenRevocationStore;

    public ResetPasswordOrchestratorHandler(
        IMediator mediator,
        IUnitOfWork unitOfWork,
        IConnectionMultiplexer redis,
        ITokenRevocationStore tokenRevocationStore)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
        _redis = redis;
        _tokenRevocationStore = tokenRevocationStore;
    }

    public async Task<Result<bool>> Handle(ResetPasswordOrchestrator request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            var emailLower = request.Email.Trim().ToLowerInvariant();
            var otpTrimmed = request.Otp.Trim();
            var db = _redis.GetDatabase();

            var otpVerifiedKey = $"auth:verified-otp:{emailLower}:{otpTrimmed}";
            var pwdResetKey = $"auth:pwd-reset:{emailLower}";

            bool isPreVerified = false;
            try
            {
                var verifiedVal = await db.StringGetAsync(otpVerifiedKey);
                var resetTokenVal = await db.StringGetAsync(pwdResetKey);

                if (verifiedVal.HasValue || (resetTokenVal.HasValue && resetTokenVal.ToString() == otpTrimmed))
                {
                    isPreVerified = true;
                    // Clean up one-time verification tokens
                    await db.KeyDeleteAsync(new RedisKey[] { otpVerifiedKey, pwdResetKey });
                }
            }
            catch
            {
                // Fall back to orchestrator verification if Redis is unavailable
            }

            if (!isPreVerified)
            {
                var verifyOtpResult = await _mediator.Send(
                    new VerifyOtpOrchestrator(request.Email, request.Otp),
                    cancellationToken);

                if (verifyOtpResult.IsFailure)
                    return Result<bool>.Failure(verifyOtpResult.Errors);
            }

            // Update user password
            var updatePasswordResult = await _mediator.Send(
                new UpdateUserPasswordCommand(request.Email, request.NewPassword),
                cancellationToken);

            if (updatePasswordResult.IsFailure)
                return Result<bool>.Failure(updatePasswordResult.Errors);

            // Fetch user to revoke all existing sessions
            var userResult = await _mediator.Send(new GetUserByEmailQuery(request.Email), cancellationToken);
            if (userResult.IsSuccess && Guid.TryParse(userResult.Value.Id, out var userGuid))
            {
                // 1. Revoke all active refresh tokens in SQL Server
                await _mediator.Send(new RevokeAllRefreshTokensCommand(userResult.Value.Id), cancellationToken);

                // 2. Invalidate all existing JWT access tokens in Redis
                await _tokenRevocationStore.RevokeAllUserTokensAsync(
                    userGuid,
                    DateTimeOffset.UtcNow,
                    TimeSpan.FromMinutes(15),
                    cancellationToken);
            }

            return Result<bool>.Success(true);
        }, cancellationToken);
    }
}
