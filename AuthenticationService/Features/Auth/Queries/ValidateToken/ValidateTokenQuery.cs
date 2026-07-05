using AuthenticationService.Common;
using MediatR;

namespace AuthenticationService.Features.Auth.Queries.ValidateToken;

public record ValidateTokenQuery(string Token) : IRequest<Result>;