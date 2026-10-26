using MediatR;
using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.Biometrics.Queries.GetUserHeight;

public record GetUserHeightQuery(Guid UserId) : IRequest<Result<double?>>;

public class GetUserHeightHandler : IRequestHandler<GetUserHeightQuery, Result<double?>>
{
    private readonly IGeneralRepo<UserBiometrics> _repo;

    public GetUserHeightHandler(IGeneralRepo<UserBiometrics> repo)
    {
        _repo = repo;
    }

    public async Task<Result<double?>> Handle(GetUserHeightQuery request, CancellationToken cancellationToken)
    {
        var height = await _repo.Find(b => b.UserId == request.UserId)
            .Select(b => (double?)b.HeightCm)
            .FirstOrDefaultAsync(cancellationToken);

        return Result.Success(height);
    }
}
