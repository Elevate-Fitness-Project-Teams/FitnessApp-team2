using MediatR;
using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.Progress.Queries.ViewUserProgress;

public record GetUserStatisticsQuery(Guid UserId) : IRequest<Result<UserStatisticDto?>>;

public class GetUserStatisticsQueryHandler : IRequestHandler<GetUserStatisticsQuery, Result<UserStatisticDto?>>
{
    private readonly IGeneralRepo<UserStatistic> _userStatsRepo;

    public GetUserStatisticsQueryHandler(IGeneralRepo<UserStatistic> userStatsRepo)
    {
        _userStatsRepo = userStatsRepo;
    }

    public async Task<Result<UserStatisticDto?>> Handle(GetUserStatisticsQuery request, CancellationToken cancellationToken)
    {
        var result = await _userStatsRepo.Find(s => s.UserId == request.UserId)
            .AsNoTracking()
            .Select(stat => new UserStatisticDto
            {
                TotalWorkouts = stat.TotalWorkouts,
                TotalCaloriesBurned = stat.TotalCaloriesBurned,
                TotalWeightLost = stat.TotalWeightLost,
                UpdatedAt = stat.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
            
        if (result is null)
        {
            return Result<UserStatisticDto?>.Failure(Error.NotFound("USER_STATISTICS_NOT_FOUND", "User statistics not found"));
        }
            
        return Result<UserStatisticDto?>.Success(result);
    }
}
