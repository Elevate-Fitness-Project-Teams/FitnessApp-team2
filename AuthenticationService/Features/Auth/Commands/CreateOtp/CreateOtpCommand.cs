using AuthenticationService.Common;
using MediatR;

namespace AuthenticationService.Features.Auth.Commands.CreateOtp;

public record CreateOtpCommand(string Email) : IRequest<Result>;
