using MediatR;
using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Features.Achievements.DTOs;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.Achievements.Queries.GetAchievementProgressSnapshot;

public record GetAchievementProgressSnapshotQuery(Guid UserId) : IRequest<Result<AchievementProgressSnapshotDto>>;

public class GetAchievementProgressSnapshotHandler : IRequestHandler<GetAchievementProgressSnapshotQuery, Result<AchievementProgressSnapshotDto>>
{
    private readonly IGeneralRepo<UserStatistic> _statsRepo;
    private readonly IGeneralRepo<Streak> _streakRepo;

    public GetAchievementProgressSnapshotHandler(
        IGeneralRepo<UserStatistic> statsRepo,
        IGeneralRepo<Streak> streakRepo)
    {
        _statsRepo = statsRepo;
        _streakRepo = streakRepo;
    }

    public async Task<Result<AchievementProgressSnapshotDto>> Handle(
        GetAchievementProgressSnapshotQuery request,
        CancellationToken cancellationToken)
    {
        var stats = await _statsRepo.Find(s => s.UserId == request.UserId)
            .Select(s => new
            {
                s.TotalWorkouts,
                s.TotalCaloriesBurned,
                s.TotalWeightLost
            })
            .FirstOrDefaultAsync(cancellationToken);

        var streak = await _streakRepo.Find(s => s.UserId == request.UserId)
            .Select(s => s.CurrentStreak)
            .FirstOrDefaultAsync(cancellationToken);

        var snapshot = new AchievementProgressSnapshotDto(
            TotalWorkouts: stats?.TotalWorkouts ?? 0,
            CurrentStreak: streak,
            TotalCaloriesBurned: stats?.TotalCaloriesBurned ?? 0,
            TotalWeightLost: stats?.TotalWeightLost ?? 0.0);

        return Result.Success(snapshot);
    }
}
