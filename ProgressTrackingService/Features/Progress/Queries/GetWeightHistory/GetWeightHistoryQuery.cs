using MediatR;
using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.Progress.Queries.ViewUserProgress;

public record GetWeightHistoryQuery(Guid UserId) : IRequest<Result<List<WeightHistoryDto>>>;

public class GetWeightHistoryQueryHandler : IRequestHandler<GetWeightHistoryQuery, Result<List<WeightHistoryDto>>>
{
    private readonly IGeneralRepo<WeightHistory> _weightHistoryRepo;

    public GetWeightHistoryQueryHandler(IGeneralRepo<WeightHistory> weightHistoryRepo)
    {
        _weightHistoryRepo = weightHistoryRepo;
    }

    public async Task<Result<List<WeightHistoryDto>>> Handle(GetWeightHistoryQuery request, CancellationToken cancellationToken)
    {
        var result = await _weightHistoryRepo.Find(w => w.UserId == request.UserId)
            .AsNoTracking()
            .OrderByDescending(w => w.Date)
            .Select(wh => new WeightHistoryDto
            {
                Id = wh.Id,
                Weight = wh.Weight,
                Date = wh.Date,
                Notes = wh.Notes
            })
            .ToListAsync(cancellationToken);
            
        return Result<List<WeightHistoryDto>>.Success(result);
    }
}
