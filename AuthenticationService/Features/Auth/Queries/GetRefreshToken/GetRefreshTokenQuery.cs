using AuthenticationService.Models.Responses;
using MediatR;

namespace AuthenticationService.Features.Auth.Queries.GetRefreshToken;

public record GetRefreshTokenQuery(string Token) : IRequest<RefreshTokenDto?>;