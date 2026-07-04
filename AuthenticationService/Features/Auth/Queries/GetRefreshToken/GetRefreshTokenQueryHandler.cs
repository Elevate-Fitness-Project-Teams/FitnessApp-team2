using AuthenticationService.Data;
using AuthenticationService.Data.Entities;
using AuthenticationService.Models.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationService.Features.Auth.Queries.GetRefreshToken;

public class GetRefreshTokenQueryHandler : IRequestHandler<GetRefreshTokenQuery, RefreshTokenDto?>
{
    private readonly IGeneralRepo<RefreshToken> _repo;

    public GetRefreshTokenQueryHandler(IGeneralRepo<RefreshToken> repo)
    {
        _repo = repo;
    }

    public async Task<RefreshTokenDto?> Handle(GetRefreshTokenQuery request, CancellationToken cancellationToken)
    {
        var tokens = await _repo.Find(x => x.Token == request.Token).ToListAsync(cancellationToken);
        var token = tokens.FirstOrDefault();

        if (token == null) return null;

        return new RefreshTokenDto(
            token.Id,
            token.UserId,
            token.Token,
            token.ExpiresAt,
            token.RevokedAt);
    }
}