using AuthenticationService.Common;
using MediatR;

namespace AuthenticationService.Features.Auth.Commands.UpdateUserLockout;

public record UpdateUserLockoutCommand(string Email, bool IsLockedOut, DateTime? LockedUntil) : IRequest<Result>;