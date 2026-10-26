using MediatR;
using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.Progress.Queries.GetWeightBeforeDate;

public record GetWeightBeforeDateQuery(Guid UserId, DateTime Date) : IRequest<Result<double?>>;

public class GetWeightBeforeDateHandler : IRequestHandler<GetWeightBeforeDateQuery, Result<double?>>
{
    private readonly IGeneralRepo<WeightHistory> _repo;

    public GetWeightBeforeDateHandler(IGeneralRepo<WeightHistory> repo)
    {
        _repo = repo;
    }

    public async Task<Result<double?>> Handle(GetWeightBeforeDateQuery request, CancellationToken cancellationToken)
    {
        var previous = await _repo.Find(w => w.UserId == request.UserId && w.Date < request.Date)
            .OrderByDescending(w => w.Date)
            .Select(w => (double?)w.Weight)
            .FirstOrDefaultAsync(cancellationToken);

        return Result.Success(previous);
    }
}
