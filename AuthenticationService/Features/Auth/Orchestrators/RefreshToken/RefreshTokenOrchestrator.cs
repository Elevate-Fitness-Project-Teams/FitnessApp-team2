using AuthenticationService.Common;
using MediatR;

namespace AuthenticationService.Features.Auth.Orchestrators.RefreshToken;

public record RefreshTokenOrchestrator(string RefreshToken) : IRequest<Result<RefreshTokenOrchestratorResponse>>;

public record RefreshTokenOrchestratorResponse(string AccessToken, string RefreshToken);