using MediatR;
using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.Progress.Queries.GetPreviousWeight;

public record GetPreviousWeightQuery(Guid UserId) : IRequest<Result<double?>>;

public class GetPreviousWeightQueryHandler : IRequestHandler<GetPreviousWeightQuery, Result<double?>>
{
    private readonly IGeneralRepo<WeightHistory> _weightHistoryRepo;

    public GetPreviousWeightQueryHandler(IGeneralRepo<WeightHistory> weightHistoryRepo)
    {
        _weightHistoryRepo = weightHistoryRepo;
    }

    public async Task<Result<double?>> Handle(GetPreviousWeightQuery request, CancellationToken cancellationToken)
    {
        var previousWeight = await _weightHistoryRepo.Find(w => w.UserId == request.UserId)
            .AsNoTracking()
            .OrderByDescending(w => w.Date)
            .Select(w => (double?)w.Weight)
            .FirstOrDefaultAsync(cancellationToken);

        return Result<double?>.Success(previousWeight);
    }
}
