using MediatR;
using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.WorkoutLogs.Commands.UpdateStatistic;

public class UpdateStatisticCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
    public int CaloriesBurned { get; set; }
}

public class UpdateStatisticHandler : IRequestHandler<UpdateStatisticCommand, Result>
{
    private readonly IGeneralRepo<UserStatistic> _statisticRepo;

    public UpdateStatisticHandler(IGeneralRepo<UserStatistic> statisticRepo)
    {
        _statisticRepo = statisticRepo;
    }

    public async Task<Result> Handle(UpdateStatisticCommand request, CancellationToken ct)
    {
        var stats = await _statisticRepo.Find(s => s.UserId == request.UserId).AsTracking().FirstOrDefaultAsync(ct);
        if (stats == null)
        {
            stats = new UserStatistic
            {
                UserId = request.UserId,
                TotalWorkouts = 1,
                TotalCaloriesBurned = request.CaloriesBurned,
                UpdatedAt = DateTime.UtcNow
            };
            await _statisticRepo.AddAsync(stats, ct);
        }
        else
        {
            stats.TotalWorkouts++;
            stats.TotalCaloriesBurned += request.CaloriesBurned;
            stats.UpdatedAt = DateTime.UtcNow;
        }
        return Result.Success();
    }
}
