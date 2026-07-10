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
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStreakHandler(IGeneralRepo<Streak> streakRepo, IUnitOfWork unitOfWork)
    {
        _streakRepo = streakRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateStreakCommand request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            var streak = await _streakRepo.Find(s => s.UserId == request.UserId)
            .Select(s => new Streak 
            {
                UserId = s.UserId,
                CurrentStreak = s.CurrentStreak,
                LongestStreak = s.LongestStreak,
                LastWorkoutDate = s.LastWorkoutDate
            })
            .FirstOrDefaultAsync(cancellationToken);
            
            if (streak == null)
            {
                streak = new Streak
                {
                    UserId = request.UserId, CurrentStreak = 1, LongestStreak = 1, LastWorkoutDate = request.CompletedAt
                };
                await _streakRepo.AddAsync(streak, cancellationToken);
            }
            else
            {
                if (streak.LastWorkoutDate.HasValue)
                {
                    var lastDate = streak.LastWorkoutDate.Value.Date;
                    var currentDate = request.CompletedAt.Date;
                    var daysDiff = (currentDate - lastDate).TotalDays;

                    if (daysDiff == 1)
                    {
                        streak.CurrentStreak++;
                        if (streak.CurrentStreak > streak.LongestStreak)
                            streak.LongestStreak = streak.CurrentStreak;
                    }
                    else if (daysDiff > 1)
                    {
                        streak.CurrentStreak = 1;
                    }
                }
                else
                {
                    streak.CurrentStreak = 1;
                    if (streak.CurrentStreak > streak.LongestStreak)
                        streak.LongestStreak = streak.CurrentStreak;
                }

                streak.LastWorkoutDate = request.CompletedAt;
                _streakRepo.SaveInclude(streak, nameof(Streak.CurrentStreak), nameof(Streak.LongestStreak),
                    nameof(Streak.LastWorkoutDate));
            }

            return Result.Success();
        }, cancellationToken);
    }
}