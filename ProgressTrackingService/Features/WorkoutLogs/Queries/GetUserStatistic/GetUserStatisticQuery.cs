using MediatR;
using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.WorkoutLogs.Queries.GetUserStatistic;

public class GetUserStatisticQuery : IRequest<Result<GetUserStatisticResponse>>
{
    public Guid UserId { get; set; }
}

public class GetUserStatisticHandler : IRequestHandler<GetUserStatisticQuery, Result<GetUserStatisticResponse>>
{
    private readonly IGeneralRepo<UserStatistic> _statisticRepo;

    public GetUserStatisticHandler(IGeneralRepo<UserStatistic> statisticRepo)
    {
        _statisticRepo = statisticRepo;
    }

    public async Task<Result<GetUserStatisticResponse>> Handle(GetUserStatisticQuery request, CancellationToken cancellationToken)
    {
        var stats = await _statisticRepo.Find(s => s.UserId == request.UserId)
            .Select(stats => new GetUserStatisticResponse
            {
                UserId = stats.UserId,
                TotalWorkouts = stats.TotalWorkouts,
                TotalCaloriesBurned = stats.TotalCaloriesBurned,
                UpdatedAt = stats.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (stats == null)
        {
            return Result<GetUserStatisticResponse>.Failure(Error.NotFound("RES_NOT_FOUND", "Statistics not found for user."));
        }

        return Result<GetUserStatisticResponse>.Success(stats);
    }
}
