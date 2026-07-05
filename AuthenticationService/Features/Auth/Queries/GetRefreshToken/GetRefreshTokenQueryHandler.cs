using AuthenticationService.Common;
using AuthenticationService.Data;
using AuthenticationService.Data.Entities;
using AuthenticationService.Models.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationService.Features.Auth.Queries.GetRefreshToken;

public class GetRefreshTokenQueryHandler : IRequestHandler<GetRefreshTokenQuery, Result<RefreshTokenDto>>
{
    private readonly IGeneralRepo<RefreshToken> _repo;

    public GetRefreshTokenQueryHandler(IGeneralRepo<RefreshToken> repo)
    {
        _repo = repo;
    }

    public async Task<Result<RefreshTokenDto>> Handle(GetRefreshTokenQuery request, CancellationToken cancellationToken)
    {
        var token = await _repo.Find(x => x.Token == request.Token)
            .Select(t => new RefreshTokenDto(
                t.Id,
                t.UserId,
                t.Token,
                t.ExpiresAt,
                t.RevokedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (token == null) return Result<RefreshTokenDto>.Failure(Error.Failure("AUTH_INVALID_REFRESH_TOKEN", "Refresh token not found"));

        return Result<RefreshTokenDto>.Success(token);
    }
}