using AuthenticationService.Common;
using AuthenticationService.Data;
using AuthenticationService.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationService.Features.Auth.Commands.RevokeRefreshToken;

public class RevokeRefreshTokenCommandHandler : IRequestHandler<RevokeRefreshTokenCommand, Result>
{
    private readonly IGeneralRepo<RefreshToken> _repo;
    private readonly IUnitOfWork _unitOfWork;

    public RevokeRefreshTokenCommandHandler(IGeneralRepo<RefreshToken> repo, IUnitOfWork unitOfWork)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            var token = await _repo.Find(x => x.Token == request.Token)
                .Select(x => new RefreshToken { Id = x.Id, RevokedAt = x.RevokedAt })
                .FirstOrDefaultAsync(cancellationToken);

            if (token != null && token.RevokedAt == null)
            {
                token.RevokedAt = DateTime.UtcNow;
                _repo.SaveInclude(token, nameof(token.RevokedAt));
            }

            return Result.Success();
        }, cancellationToken);
    }
}