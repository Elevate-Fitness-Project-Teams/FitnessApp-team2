using AuthenticationService.Common;
using AuthenticationService.Data;
using AuthenticationService.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationService.Features.Auth.Commands.MarkOtpAsUsed;

public class MarkOtpAsUsedCommandHandler : IRequestHandler<MarkOtpAsUsedCommand, Result>
{
    private readonly IGeneralRepo<OtpCode> _otpRepo;
    private readonly IUnitOfWork _unitOfWork;

    public MarkOtpAsUsedCommandHandler(IGeneralRepo<OtpCode> otpRepo, IUnitOfWork unitOfWork)
    {
        _otpRepo = otpRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(MarkOtpAsUsedCommand request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            var otps = await _otpRepo
                                    .Find(o => o.Email == request.Email && o.Code == request.Otp && !o.IsUsed)
                                    .ToListAsync(cancellationToken);
            var otpRecord = otps.OrderByDescending(o => o.Id).FirstOrDefault();

            if (otpRecord == null || otpRecord.ExpiresAt < DateTime.UtcNow)
                return Result.Failure(Error.Failure("AUTH_INVALID_CREDENTIALS",
                    "Invalid or expired OTP."));

            // Mark OTP as used
            otpRecord.IsUsed = true;
            _otpRepo.Update(otpRecord);

            return Result.Success();
        }, cancellationToken);
    }
}
