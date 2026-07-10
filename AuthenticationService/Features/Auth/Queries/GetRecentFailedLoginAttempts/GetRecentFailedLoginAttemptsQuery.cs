using AuthenticationService.Common;
using MediatR;

namespace AuthenticationService.Features.Auth.Queries.GetRecentFailedLoginAttempts;

public record GetRecentFailedLoginAttemptsQuery(string Email, DateTime CutoffTime) : IRequest<Result<int>>;