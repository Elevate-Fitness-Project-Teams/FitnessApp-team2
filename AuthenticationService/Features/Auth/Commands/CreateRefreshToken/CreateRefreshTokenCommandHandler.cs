using AuthenticationService.Data;
using AuthenticationService.Data.Entities;
using MediatR;

namespace AuthenticationService.Features.Auth.Commands.CreateRefreshToken;

public class CreateRefreshTokenCommandHandler : IRequestHandler<CreateRefreshTokenCommand>
{
    private readonly IGeneralRepo<RefreshToken> _repo;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRefreshTokenCommandHandler(IGeneralRepo<RefreshToken> repo, IUnitOfWork unitOfWork)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CreateRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.ExecuteAsync(async () =>
        {
            var refreshToken = new RefreshToken
            {
                UserId = request.UserId,
                Token = request.Token,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = request.ExpiresAt
            };

            await _repo.AddAsync(refreshToken, cancellationToken);
        }, cancellationToken);
    }
}