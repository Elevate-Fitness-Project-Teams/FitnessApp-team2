using AuthenticationService.Common;
using AuthenticationService.Data;
using AuthenticationService.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace AuthenticationService.Features.Auth.Commands.MarkOtpAsUsed;

public class MarkOtpAsUsedCommandHandler : IRequestHandler<MarkOtpAsUsedCommand, Result>
{
    private readonly IGeneralRepo<OtpCode> _otpRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConnectionMultiplexer _redis;

    public MarkOtpAsUsedCommandHandler(
        IGeneralRepo<OtpCode> otpRepo,
        IUnitOfWork unitOfWork,
        IConnectionMultiplexer redis)
    {
        _otpRepo = otpRepo;
        _unitOfWork = unitOfWork;
        _redis = redis;
    }

    public async Task<Result> Handle(MarkOtpAsUsedCommand request, CancellationToken cancellationToken)
    {
        var db = _redis.GetDatabase();
        var emailLower = request.Email.Trim().ToLowerInvariant();
        var attemptsKey = $"auth:otp-attempts:{emailLower}";

        try
        {
            var attemptsVal = await db.StringGetAsync(attemptsKey);
            if (attemptsVal.HasValue && long.TryParse(attemptsVal, out var currentAttempts) && currentAttempts >= 5)
            {
                return Result.Failure(Error.Failure("AUTH_OTP_MAX_ATTEMPTS",
                    "Maximum verification attempts exceeded. Please request a new OTP."));
            }
        }
        catch
        {
            // Redis error should not block user if Redis is temporarily unreachable
        }

        return await _unitOfWork.ExecuteAsync(async () =>
        {
            var otpRecord = await _otpRepo
                .Find(o => o.Email == request.Email && o.Code == request.Otp && !o.IsUsed)
                .OrderByDescending(o => o.Id)
                .Select(o => new OtpCode { Id = o.Id, ExpiresAt = o.ExpiresAt })
                .FirstOrDefaultAsync(cancellationToken);

            if (otpRecord == null || otpRecord.ExpiresAt < DateTime.UtcNow)
            {
                try
                {
                    var newCount = await db.StringIncrementAsync(attemptsKey);
                    if (newCount == 1)
                    {
                        await db.KeyExpireAsync(attemptsKey, TimeSpan.FromMinutes(10));
                    }

                    if (newCount >= 5)
                    {
                        return Result.Failure(Error.Failure("AUTH_OTP_MAX_ATTEMPTS",
                            "Maximum verification attempts exceeded. Please request a new OTP."));
                    }
                }
                catch
                {
                    // Redis fail-open for counters
                }

                return Result.Failure(Error.Failure("AUTH_INVALID_CREDENTIALS",
                    "Invalid or expired OTP."));
            }

            try
            {
                await db.KeyDeleteAsync(attemptsKey);
            }
            catch
            {
                // Best effort Redis clear
            }

            otpRecord.IsUsed = true;
            _otpRepo.SaveInclude(otpRecord, nameof(otpRecord.IsUsed));

            return Result.Success();
        }, cancellationToken);
    }
}
