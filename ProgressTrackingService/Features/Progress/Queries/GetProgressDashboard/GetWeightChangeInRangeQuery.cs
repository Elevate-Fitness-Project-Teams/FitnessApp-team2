using MediatR;
using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.Progress.Queries.GetProgressDashboard;

public record WeightChangeDto(double? StartWeight, double? EndWeight, double? WeightChange);

public record GetWeightChangeInRangeQuery(Guid UserId, DateTime From, DateTime To) : IRequest<Result<WeightChangeDto>>;

public class GetWeightChangeInRangeQueryHandler : IRequestHandler<GetWeightChangeInRangeQuery, Result<WeightChangeDto>>
{
    private readonly IGeneralRepo<WeightHistory> _weightRepo;

    public GetWeightChangeInRangeQueryHandler(IGeneralRepo<WeightHistory> weightRepo)
    {
        _weightRepo = weightRepo;
    }

    public async Task<Result<WeightChangeDto>> Handle(GetWeightChangeInRangeQuery request, CancellationToken cancellationToken)
    {
        var from = request.From.Date;
        var to = request.To.Date.AddDays(1).AddTicks(-1);

        var entries = await _weightRepo.Find(w => w.UserId == request.UserId && w.Date >= from && w.Date <= to)
            .AsNoTracking()
            .OrderBy(w => w.Date)
            .Select(w => w.Weight)
            .ToListAsync(cancellationToken);

        if (entries.Count == 0)
        {
            return Result.Success(new WeightChangeDto(null, null, null));
        }

        var startWeight = entries.First();
        var endWeight = entries.Last();
        var change = Math.Round(endWeight - startWeight, 2);

        return Result.Success(new WeightChangeDto(startWeight, endWeight, change));
    }
}
