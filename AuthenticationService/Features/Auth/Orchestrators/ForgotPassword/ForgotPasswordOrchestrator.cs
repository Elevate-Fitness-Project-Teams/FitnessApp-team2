using AuthenticationService.Common;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.ForgotPassword;

public record ForgotPasswordOrchestrator(string Email) : IRequest<Result<bool>>;