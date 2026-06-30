using AuthenticationService.Data;
using AuthenticationService.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationService.Features.Auth.Commands.RevokeRefreshToken;

public class RevokeRefreshTokenCommandHandler : IRequestHandler<RevokeRefreshTokenCommand, Unit>
{
    private readonly IGeneralRepo<RefreshToken> _repo;
    private readonly IUnitOfWork _unitOfWork;

    public RevokeRefreshTokenCommandHandler(IGeneralRepo<RefreshToken> repo, IUnitOfWork unitOfWork)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            var tokens = await _repo.Find(x => x.Token == request.Token).ToListAsync(cancellationToken);
            var token = tokens.FirstOrDefault();

            if (token != null && token.RevokedAt == null)
            {
                token.RevokedAt = DateTime.UtcNow;
                _repo.Update(token);
            }

            return Unit.Value;
        }, cancellationToken);
    }
}