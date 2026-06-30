using AuthenticationService.Common;
using MediatR;

namespace AuthenticationService.Features.Auth.Commands.SendOtp;

public record SendOtpCommand(string Email) : IRequest<Result<SendOtpCommandResponse>>;

public record SendOtpCommandResponse(string Email, int OtpExpiresIn, int CanResendIn);