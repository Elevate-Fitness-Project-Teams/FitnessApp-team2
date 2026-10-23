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
    public int DurationInMinutes { get; set; }
}

public class UpdateStatisticHandler : IRequestHandler<UpdateStatisticCommand, Result>
{
    private readonly IGeneralRepo<UserStatistic> _statisticRepo;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStatisticHandler(IGeneralRepo<UserStatistic> statisticRepo, IUnitOfWork unitOfWork)
    {
        _statisticRepo = statisticRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateStatisticCommand request, CancellationToken ct)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            var rowsAffected = await _statisticRepo.Find(s => s.UserId == request.UserId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.TotalWorkouts, p => p.TotalWorkouts + 1)
                    .SetProperty(p => p.TotalCaloriesBurned, p => p.TotalCaloriesBurned + request.CaloriesBurned)
                    .SetProperty(p => p.TotalMinutes, p => p.TotalMinutes + request.DurationInMinutes)
                    .SetProperty(p => p.UpdatedAt, DateTime.UtcNow), ct);

            if (rowsAffected == 0)
            {
                var newStats = new UserStatistic
                {
                    UserId = request.UserId,
                    TotalWorkouts = 1,
                    TotalCaloriesBurned = request.CaloriesBurned,
                    TotalMinutes = request.DurationInMinutes,
                    TotalWeightLost = 0,
                    UpdatedAt = DateTime.UtcNow
                };
                await _statisticRepo.AddAsync(newStats, ct);
            }

            return Result.Success();
        }, ct);
    }
}
