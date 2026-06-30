using AuthenticationService.Common;
using AuthenticationService.Data;
using AuthenticationService.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationService.Features.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<Unit>>
{
    private readonly IGeneralRepo<RefreshToken> _refreshTokenRepo;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(
        IGeneralRepo<RefreshToken> refreshTokenRepo,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenRepo = refreshTokenRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            var refreshTokens = await _refreshTokenRepo
                .Find(rt => rt.Token == request.RefreshToken && rt.UserId == request.UserId).ToListAsync(cancellationToken);
            var refreshTokenRecord = refreshTokens.FirstOrDefault();

            if (refreshTokenRecord != null)
            {
                refreshTokenRecord.RevokedAt = DateTime.UtcNow;
                _refreshTokenRepo.Update(refreshTokenRecord);
            }

            // Return success even if token not found, as the desired end state (token is not valid) is achieved
            return Result<Unit>.Success(Unit.Value);
        }, cancellationToken);
    }
}