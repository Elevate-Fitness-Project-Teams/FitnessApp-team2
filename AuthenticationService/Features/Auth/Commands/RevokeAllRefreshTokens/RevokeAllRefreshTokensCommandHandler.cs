using AuthenticationService.Common;
using AuthenticationService.Data;
using AuthenticationService.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationService.Features.Auth.Commands.RevokeAllRefreshTokens;

public class RevokeAllRefreshTokensCommandHandler : IRequestHandler<RevokeAllRefreshTokensCommand, Result>
{
    private readonly IGeneralRepo<RefreshToken> _repo;
    private readonly IUnitOfWork _unitOfWork;

    public RevokeAllRefreshTokensCommandHandler(IGeneralRepo<RefreshToken> repo, IUnitOfWork unitOfWork)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RevokeAllRefreshTokensCommand request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            var activeTokens = await _repo.Find(x => x.UserId == request.UserId && x.RevokedAt == null)
                .Select(x => new RefreshToken { Id = x.Id, RevokedAt = x.RevokedAt })
                .ToListAsync(cancellationToken);

            var now = DateTime.UtcNow;
            foreach (var token in activeTokens)
            {
                token.RevokedAt = now;
                _repo.SaveInclude(token, nameof(token.RevokedAt));
            }

            return Result.Success();
        }, cancellationToken);
    }
}
