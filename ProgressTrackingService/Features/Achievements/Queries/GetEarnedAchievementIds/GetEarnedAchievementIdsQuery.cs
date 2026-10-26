using MediatR;
using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.Achievements.Queries.GetEarnedAchievementIds;

public record GetEarnedAchievementIdsQuery(Guid UserId) : IRequest<Result<HashSet<Guid>>>;

public class GetEarnedAchievementIdsHandler : IRequestHandler<GetEarnedAchievementIdsQuery, Result<HashSet<Guid>>>
{
    private readonly IGeneralRepo<UserAchievement> _repo;

    public GetEarnedAchievementIdsHandler(IGeneralRepo<UserAchievement> repo)
    {
        _repo = repo;
    }

    public async Task<Result<HashSet<Guid>>> Handle(
        GetEarnedAchievementIdsQuery request,
        CancellationToken cancellationToken)
    {
        var earnedIds = await _repo.Find(ua => ua.UserId == request.UserId)
            .Select(ua => ua.AchievementId)
            .ToListAsync(cancellationToken);

        return Result.Success(new HashSet<Guid>(earnedIds));
    }
}
