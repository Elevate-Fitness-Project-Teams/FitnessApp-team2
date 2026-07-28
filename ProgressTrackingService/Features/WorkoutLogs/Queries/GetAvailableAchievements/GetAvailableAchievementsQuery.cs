using MediatR;
using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.WorkoutLogs.Queries.GetAvailableAchievements;

public class GetAvailableAchievementsQuery : IRequest<Result<List<GetAvailableAchievementResponse>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class GetAvailableAchievementsHandler : IRequestHandler<GetAvailableAchievementsQuery, Result<List<GetAvailableAchievementResponse>>>
{
    private readonly IGeneralRepo<Achievement> _achievementRepo;

    public GetAvailableAchievementsHandler(IGeneralRepo<Achievement> achievementRepo)
    {
        _achievementRepo = achievementRepo;
    }

    public async Task<Result<List<GetAvailableAchievementResponse>>> Handle(GetAvailableAchievementsQuery request, CancellationToken cancellationToken)
    {
        var list = await _achievementRepo.Find(a => true)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new GetAvailableAchievementResponse
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                IconUrl = a.IconUrl
            })
            .ToListAsync(cancellationToken);

        return Result<List<GetAvailableAchievementResponse>>.Success(list);
    }
}
