using AuthenticationService.Common;
using AuthenticationService.Data;
using AuthenticationService.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace AuthenticationService.Features.Auth.Commands.CreateOtp;

public class CreateOtpCommandHandler : IRequestHandler<CreateOtpCommand, Result>
{
    private readonly IGeneralRepo<OtpCode> _otpRepo;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOtpCommandHandler(IGeneralRepo<OtpCode> otpRepo, IUnitOfWork unitOfWork)
    {
        _otpRepo = otpRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CreateOtpCommand request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            var lastOtp = await _otpRepo.Find(o => o.Email == request.Email)
                .OrderByDescending(o => o.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastOtp != null)
            {
                var createdTime = lastOtp.ExpiresAt.AddMinutes(-10);
                var secondsSinceLastOtp = (DateTime.UtcNow - createdTime).TotalSeconds;
                if (secondsSinceLastOtp < 30)
                    return Result.Failure(Error.Failure(AuthErrorCodes.RateLimitExceeded,
                        "Please wait 30 seconds before requesting a new OTP."));
            }

            await _otpRepo.Find(o => o.Email == request.Email && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
                .ExecuteUpdateAsync(s => s.SetProperty(o => o.IsUsed, true), cancellationToken);

            var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

            var otpCode = new OtpCode
            {
                Email = request.Email,
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };

            await _otpRepo.AddAsync(otpCode);

            return Result.Success();
        }, cancellationToken);
    }
}
