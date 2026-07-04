using AuthenticationService.Common;
using MediatR;

namespace AuthenticationService.Features.Auth.Commands.MarkOtpAsUsed;

public record MarkOtpAsUsedCommand(string Email, string Otp) : IRequest<Result>;
