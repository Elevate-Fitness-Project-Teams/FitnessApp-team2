using AuthenticationService.Common;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.ResetPassword;

public record ResetPasswordOrchestrator(string Email, string Otp, string NewPassword) : IRequest<Result<bool>>;