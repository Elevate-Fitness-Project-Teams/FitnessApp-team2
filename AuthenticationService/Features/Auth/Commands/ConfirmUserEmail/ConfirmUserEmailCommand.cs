using AuthenticationService.Common;
using MediatR;

namespace AuthenticationService.Features.Auth.Commands.ConfirmUserEmail;

public record ConfirmUserEmailCommand(string Email) : IRequest<Result>;