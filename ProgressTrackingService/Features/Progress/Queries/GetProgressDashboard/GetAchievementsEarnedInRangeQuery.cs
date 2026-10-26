using MediatR;
using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Features.Achievements.DTOs;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.Progress.Queries.GetProgressDashboard;

public record GetAchievementsEarnedInRangeQuery(Guid UserId, DateTime From, DateTime To) : IRequest<Result<List<UserAchievementDto>>>;

public class GetAchievementsEarnedInRangeQueryHandler : IRequestHandler<GetAchievementsEarnedInRangeQuery, Result<List<UserAchievementDto>>>
{
    private readonly IGeneralRepo<UserAchievement> _userAchievementRepo;

    public GetAchievementsEarnedInRangeQueryHandler(IGeneralRepo<UserAchievement> userAchievementRepo)
    {
        _userAchievementRepo = userAchievementRepo;
    }

    public async Task<Result<List<UserAchievementDto>>> Handle(GetAchievementsEarnedInRangeQuery request, CancellationToken cancellationToken)
    {
        var from = request.From.Date;
        var to = request.To.Date.AddDays(1).AddTicks(-1);

        var list = await _userAchievementRepo.Find(ua => ua.UserId == request.UserId && ua.EarnedAt >= from && ua.EarnedAt <= to)
            .Include(ua => ua.Achievement)
            .AsNoTracking()
            .OrderByDescending(ua => ua.EarnedAt)
            .Select(ua => new UserAchievementDto
            {
                Id = ua.AchievementId,
                Code = ua.Achievement.Code,
                Name = ua.Achievement.Name,
                Description = ua.Achievement.Description,
                IconUrl = ua.Achievement.IconUrl,
                CriteriaType = ua.Achievement.CriteriaType,
                Threshold = ua.Achievement.Threshold,
                Earned = true,
                EarnedAt = ua.EarnedAt
            })
            .ToListAsync(cancellationToken);

        return Result.Success(list);
    }
}
