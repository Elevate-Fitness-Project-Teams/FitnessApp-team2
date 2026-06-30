using AuthenticationService.Data;
using AuthenticationService.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationService.Features.Auth.Queries.GetRefreshToken;

public class GetRefreshTokenQueryHandler : IRequestHandler<GetRefreshTokenQuery, RefreshToken?>
{
    private readonly IGeneralRepo<RefreshToken> _repo;

    public GetRefreshTokenQueryHandler(IGeneralRepo<RefreshToken> repo)
    {
        _repo = repo;
    }

    public async Task<RefreshToken?> Handle(GetRefreshTokenQuery request, CancellationToken cancellationToken)
    {
        var tokens = await _repo.Find(x => x.Token == request.Token).ToListAsync(cancellationToken);
        return tokens.FirstOrDefault();
    }
}