using AuthenticationService.Common;
using AuthenticationService.Data;
using AuthenticationService.Data.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationService.Features.Auth.Commands.VerifyOtp;

public class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, Result<VerifyOtpCommandResponse>>
{
    private readonly IGeneralRepo<OtpCode> _otpRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;

    public VerifyOtpCommandHandler(
        IGeneralRepo<OtpCode> otpRepo,
        IUnitOfWork unitOfWork,
        UserManager<User> userManager)
    {
        _otpRepo = otpRepo;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<Result<VerifyOtpCommandResponse>> Handle(VerifyOtpCommand request,
        CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            var otps = await _otpRepo.Find(o => o.Email == request.Email && o.Code == request.Otp && !o.IsUsed)
                .ToListAsync(cancellationToken);
            var otpRecord = otps.OrderByDescending(o => o.Id).FirstOrDefault();

            if (otpRecord == null || otpRecord.ExpiresAt < DateTime.UtcNow)
                return Result<VerifyOtpCommandResponse>.Failure(Error.Failure("AUTH_INVALID_CREDENTIALS",
                    "Invalid or expired OTP."));

            // Mark OTP as used
            otpRecord.IsUsed = true;
            _otpRepo.Update(otpRecord);

            // Confirm User Email
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user != null && !user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
            }

            // Generate a short-lived reset token
            var resetToken = Guid.NewGuid().ToString("N");

            // TODO: In a complete implementation, this resetToken should be stored (e.g. in Redis or a DB table) 
            // linked to the user's email with a short expiry (like 15 minutes) to authorize a password reset call.

            return Result<VerifyOtpCommandResponse>.Success(new VerifyOtpCommandResponse(resetToken));
        }, cancellationToken);
    }
}