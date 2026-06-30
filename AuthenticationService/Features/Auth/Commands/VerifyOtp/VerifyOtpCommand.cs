using AuthenticationService.Common;
using MediatR;

namespace AuthenticationService.Features.Auth.Commands.VerifyOtp;

public record VerifyOtpCommand(string Email, string Otp) : IRequest<Result<VerifyOtpCommandResponse>>;

public record VerifyOtpCommandResponse(string ResetToken);