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
            var otpRecord = await _otpRepo
                .Find(o => o.Email == request.Email && o.Code == request.Otp && !o.IsUsed)
                .OrderByDescending(o => o.Id)
                .Select(o => new OtpCode { Id = o.Id, ExpiresAt = o.ExpiresAt })
                .FirstOrDefaultAsync(cancellationToken);

            if (otpRecord == null || otpRecord.ExpiresAt < DateTime.UtcNow)
                return Result.Failure(Error.Failure("AUTH_INVALID_CREDENTIALS",
                    "Invalid or expired OTP."));

            otpRecord.IsUsed = true;
            _otpRepo.SaveInclude(otpRecord, nameof(otpRecord.IsUsed));

            return Result.Success();
        }, cancellationToken);
    }
}