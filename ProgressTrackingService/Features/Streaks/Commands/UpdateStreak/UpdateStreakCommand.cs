using MediatR;
using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.Streaks.Commands.UpdateStreak;

public class UpdateStreakCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
    public DateTime CompletedAt { get; set; }
}

public class UpdateStreakHandler : IRequestHandler<UpdateStreakCommand, Result>
{
    private readonly IGeneralRepo<Streak> _streakRepo;

    public UpdateStreakHandler(IGeneralRepo<Streak> streakRepo)
    {
        _streakRepo = streakRepo;
    }

    public async Task<Result> Handle(UpdateStreakCommand request, CancellationToken ct)
    {
        var streak = await _streakRepo.Find(s => s.UserId == request.UserId).AsTracking().FirstOrDefaultAsync(ct);

        if (streak == null)
        {
            streak = new Streak { UserId = request.UserId, CurrentStreak = 1, LongestStreak = 1, LastWorkoutDate = request.CompletedAt };
            await _streakRepo.AddAsync(streak, ct);
        }
        else
        {
            if (streak.LastWorkoutDate.HasValue)
            {
                var daysDiff = (request.CompletedAt.Date - streak.LastWorkoutDate.Value.Date).TotalDays;
                if (daysDiff == 1)
                {
                    streak.CurrentStreak++;
                    if (streak.CurrentStreak > streak.LongestStreak) streak.LongestStreak = streak.CurrentStreak;
                }
                else if (daysDiff > 1)
                {
                    streak.CurrentStreak = 1;
                }
            }
            else
            {
                streak.CurrentStreak = 1;
                if (streak.CurrentStreak > streak.LongestStreak) streak.LongestStreak = streak.CurrentStreak;
            }
            streak.LastWorkoutDate = request.CompletedAt;
        }

        return Result.Success();
    }
}
