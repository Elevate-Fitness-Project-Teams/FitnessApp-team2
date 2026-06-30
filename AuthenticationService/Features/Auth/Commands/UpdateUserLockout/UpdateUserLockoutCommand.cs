using AuthenticationService.Data.Entities;
using MediatR;

namespace AuthenticationService.Features.Auth.Commands.UpdateUserLockout;

public record UpdateUserLockoutCommand(User User, bool IsLockedOut, DateTime? LockedUntil) : IRequest;