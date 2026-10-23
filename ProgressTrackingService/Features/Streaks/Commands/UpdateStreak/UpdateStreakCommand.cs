using MediatR;
using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Common.Domain;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.Streaks.Commands.UpdateStreak;

public class UpdateStreakCommand : IRequest<Result<StreakCalculationResult>>
{
    public Guid UserId { get; set; }
    public DateTime CompletedAt { get; set; }
}

public class UpdateStreakHandler : IRequestHandler<UpdateStreakCommand, Result<StreakCalculationResult>>
{
    private readonly IGeneralRepo<Streak> _streakRepo;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStreakHandler(IGeneralRepo<Streak> streakRepo, IUnitOfWork unitOfWork)
    {
        _streakRepo = streakRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<StreakCalculationResult>> Handle(UpdateStreakCommand request, CancellationToken ct)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            var existing = await _streakRepo.Find(s => s.UserId == request.UserId)
                .AsNoTracking()
                .Select(s => new
                {
                    s.CurrentStreak,
                    s.LongestStreak,
                    s.LastWorkoutDate
                })
                .FirstOrDefaultAsync(ct);

            var calc = StreakCalculator.Calculate(
                existing?.CurrentStreak ?? 0,
                existing?.LongestStreak ?? 0,
                existing?.LastWorkoutDate,
                request.CompletedAt);

            if (existing == null)
            {
                var newStreak = new Streak
                {
                    UserId = request.UserId,
                    CurrentStreak = calc.CurrentStreak,
                    LongestStreak = calc.LongestStreak,
                    LastWorkoutDate = request.CompletedAt
                };
                await _streakRepo.AddAsync(newStreak, ct);
            }
            else
            {
                await _streakRepo.Find(s => s.UserId == request.UserId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(p => p.CurrentStreak, calc.CurrentStreak)
                        .SetProperty(p => p.LongestStreak, calc.LongestStreak)
                        .SetProperty(p => p.LastWorkoutDate, request.CompletedAt), ct);
            }

            return Result<StreakCalculationResult>.Success(calc);
        }, ct);
    }
}
