using MediatR;

namespace AuthenticationService.Features.Auth.Commands.LogLoginAttempt;

public record LogLoginAttemptCommand(string Email, bool IsSuccess, string IpAddress) : IRequest;