using System.Security.Cryptography;
using AuthenticationService.Common;
using AuthenticationService.Data;
using AuthenticationService.Data.Entities;
using AuthenticationService.Features.Auth.Queries.GetUserByEmail;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationService.Features.Auth.Commands.SendOtp;

public class SendOtpCommandHandler : IRequestHandler<SendOtpCommand, Result<SendOtpCommandResponse>>
{
    private readonly IMediator _mediator;
    private readonly IGeneralRepo<OtpCode> _otpRepo;
    private readonly IUnitOfWork _unitOfWork;

    public SendOtpCommandHandler(
        IGeneralRepo<OtpCode> otpRepo,
        IUnitOfWork unitOfWork,
        IMediator mediator)
    {
        _otpRepo = otpRepo;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task<Result<SendOtpCommandResponse>> Handle(SendOtpCommand request,
        CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            var user = await _mediator.Send(new GetUserByEmailQuery(request.Email), cancellationToken);

            if (user == null)
                return Result<SendOtpCommandResponse>.Failure(Error.NotFound(AuthErrorCodes.UserNotFound,
                    "User not found"));

            var otps = await _otpRepo.Find(o => o.Email == request.Email).ToListAsync(cancellationToken);
            var lastOtp = otps.OrderByDescending(o => o.Id).FirstOrDefault();

            if (lastOtp != null)
            {
                var createdTime = lastOtp.ExpiresAt.AddMinutes(-10);
                var secondsSinceLastOtp = (DateTime.UtcNow - createdTime).TotalSeconds;
                if (secondsSinceLastOtp < 30)
                    return Result<SendOtpCommandResponse>.Failure(Error.Failure(AuthErrorCodes.RateLimitExceeded,
                        "Please wait 30 seconds before requesting a new OTP."));
            }

            foreach (var otp in otps.Where(o => !o.IsUsed && o.ExpiresAt > DateTime.UtcNow))
            {
                otp.IsUsed = true;
                _otpRepo.Update(otp);
            }

            var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

            var otpCode = new OtpCode
            {
                Email = request.Email,
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };

            await _otpRepo.AddAsync(otpCode);

            // TODO: Publish event to send the email

            return Result<SendOtpCommandResponse>.Success(
                new SendOtpCommandResponse(request.Email, 600, 30));
        }, cancellationToken);
    }
}