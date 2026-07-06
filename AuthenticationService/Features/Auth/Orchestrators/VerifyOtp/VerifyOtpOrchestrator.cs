using AuthenticationService.Common;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.VerifyOtp;

public record VerifyOtpOrchestrator(string Email, string Otp) : IRequest<Result<VerifyOtpResponse>>;

public record VerifyOtpResponse(string ResetToken);