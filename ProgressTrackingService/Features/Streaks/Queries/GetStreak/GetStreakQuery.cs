using MediatR;
using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.Streaks.Queries.GetStreak;

public class GetStreakQuery : IRequest<Result<GetStreakResponse>>
{
    public Guid UserId { get; set; }
}

public class GetStreakHandler : IRequestHandler<GetStreakQuery, Result<GetStreakResponse>>
{
    private readonly IGeneralRepo<Streak> _streakRepo;

    public GetStreakHandler(IGeneralRepo<Streak> streakRepo)
    {
        _streakRepo = streakRepo;
    }

    public async Task<Result<GetStreakResponse>> Handle(GetStreakQuery request, CancellationToken cancellationToken)
    {
        var streak = await _streakRepo.Find(s => s.UserId == request.UserId)
            .Select(streak => new GetStreakResponse
            {
                UserId = streak.UserId,
                CurrentStreak = streak.CurrentStreak,
                LongestStreak = streak.LongestStreak,
                LastWorkoutDate = streak.LastWorkoutDate
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (streak == null)
        {
            return Result<GetStreakResponse>.Failure(Error.NotFound("RES_NOT_FOUND", "Streak not found for user."));
        }

        return Result<GetStreakResponse>.Success(streak);
    }
}
