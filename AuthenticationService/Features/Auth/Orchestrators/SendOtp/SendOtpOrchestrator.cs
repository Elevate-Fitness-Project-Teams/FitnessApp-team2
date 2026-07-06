using AuthenticationService.Common;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.SendOtp;

public record SendOtpOrchestrator(string Email) : IRequest<Result<SendOtpResponse>>;

public record SendOtpResponse(string Email, int OtpExpiresIn, int CanResendIn);