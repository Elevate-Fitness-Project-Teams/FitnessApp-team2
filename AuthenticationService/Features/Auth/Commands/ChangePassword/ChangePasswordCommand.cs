using AuthenticationService.Common;
using MediatR;

namespace AuthenticationService.Features.Auth.Commands.ChangePassword;

public record ChangePasswordCommand(string UserId, string OldPassword, string NewPassword) : IRequest<Result<bool>>;