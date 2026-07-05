using AuthenticationService.Common;
using MediatR;

namespace AuthenticationService.Features.Auth.Commands.ChangeUserPassword;

public record ChangeUserPasswordCommand(string UserId, string OldPassword, string NewPassword) : IRequest<Result>;