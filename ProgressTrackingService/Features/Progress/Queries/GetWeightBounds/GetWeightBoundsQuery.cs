using MediatR;
using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.Progress.Queries.GetWeightBounds;

public record WeightBoundsDto(double InitialWeight, double LatestWeight, bool IsNewEntryLatest);

public record GetWeightBoundsQuery(Guid UserId, DateTime NewEntryDate) : IRequest<Result<WeightBoundsDto>>;

public class GetWeightBoundsHandler : IRequestHandler<GetWeightBoundsQuery, Result<WeightBoundsDto>>
{
    private readonly IGeneralRepo<WeightHistory> _repo;

    public GetWeightBoundsHandler(IGeneralRepo<WeightHistory> repo)
    {
        _repo = repo;
    }

    public async Task<Result<WeightBoundsDto>> Handle(GetWeightBoundsQuery request, CancellationToken cancellationToken)
    {
        var query = _repo.Find(w => w.UserId == request.UserId);

        var initial = await query.OrderBy(w => w.Date)
            .Select(w => w.Weight)
            .FirstOrDefaultAsync(cancellationToken);

        var latestEntry = await query.OrderByDescending(w => w.Date)
            .Select(w => new { w.Weight, w.Date })
            .FirstOrDefaultAsync(cancellationToken);

        var latestWeight = latestEntry?.Weight ?? initial;
        var isLatest = latestEntry == null || request.NewEntryDate >= latestEntry.Date;

        return Result.Success(new WeightBoundsDto(initial, latestWeight, isLatest));
    }
}
