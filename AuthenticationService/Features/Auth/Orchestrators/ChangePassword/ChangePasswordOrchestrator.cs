using AuthenticationService.Common;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.ChangePassword;

public record ChangePasswordOrchestrator(string UserId, string OldPassword, string NewPassword)
    : IRequest<Result<bool>>;