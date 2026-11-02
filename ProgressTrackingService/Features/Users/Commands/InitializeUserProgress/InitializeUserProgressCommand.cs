using MediatR;
using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.Users.Commands.InitializeUserProgress;

public record InitializeUserProgressCommand(Guid UserId) : IRequest<Result>;

public class InitializeUserProgressCommandHandler : IRequestHandler<InitializeUserProgressCommand, Result>
{
    private readonly IGeneralRepo<Streak> _streakRepo;
    private readonly IGeneralRepo<UserStatistic> _userStatsRepo;
    private readonly IUnitOfWork _unitOfWork;

    public InitializeUserProgressCommandHandler(
        IGeneralRepo<Streak> streakRepo,
        IGeneralRepo<UserStatistic> userStatsRepo,
        IUnitOfWork unitOfWork)
    {
        _streakRepo = streakRepo;
        _userStatsRepo = userStatsRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(InitializeUserProgressCommand request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            // 1. Initialize Streak if not present
            var streakExists = await _streakRepo.Find(s => s.UserId == request.UserId)
                .AsNoTracking()
                .AnyAsync(cancellationToken);

            if (!streakExists)
            {
                var newStreak = new Streak
                {
                    UserId = request.UserId,
                    CurrentStreak = 0,
                    LongestStreak = 0,
                    LastWorkoutDate = null
                };
                await _streakRepo.AddAsync(newStreak, cancellationToken);
            }

            // 2. Initialize UserStatistic if not present
            var statsExists = await _userStatsRepo.Find(s => s.UserId == request.UserId)
                .AsNoTracking()
                .AnyAsync(cancellationToken);

            if (!statsExists)
            {
                var newStats = new UserStatistic
                {
                    UserId = request.UserId,
                    TotalWorkouts = 0,
                    TotalCaloriesBurned = 0,
                    TotalMinutes = 0,
                    InitialWeight = 0,
                    CurrentWeight = 0,
                    TotalWeightLost = 0,
                    UpdatedAt = DateTime.UtcNow
                };
                await _userStatsRepo.AddAsync(newStats, cancellationToken);
            }

            return Result.Success();
        }, cancellationToken);
    }
}
