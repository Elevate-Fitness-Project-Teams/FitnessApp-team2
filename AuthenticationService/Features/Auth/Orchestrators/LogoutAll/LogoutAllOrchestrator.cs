using AuthenticationService.Common;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.LogoutAll;

public record LogoutAllOrchestrator(string UserId) : IRequest<Result>;
