using AuthenticationService.Common;
using MediatR;

namespace AuthenticationService.Features.Auth.Commands.UpdateUserPassword;

public record UpdateUserPasswordCommand(string Email, string NewPassword) : IRequest<Result>;