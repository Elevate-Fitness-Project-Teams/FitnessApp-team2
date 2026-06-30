using AuthenticationService.Data.Entities;
using MediatR;

namespace AuthenticationService.Features.Auth.Queries.GetRefreshToken;

public record GetRefreshTokenQuery(string Token) : IRequest<RefreshToken?>;