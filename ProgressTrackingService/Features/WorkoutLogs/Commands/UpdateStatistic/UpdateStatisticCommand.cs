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
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStatisticHandler(IGeneralRepo<UserStatistic> statisticRepo, IUnitOfWork unitOfWork)
    {
        _statisticRepo = statisticRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateStatisticCommand request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            var stats = await _statisticRepo
                .Find(s => s.UserId == request.UserId)
                .Select(s => new UserStatistic
                {
                    UserId = s.UserId,
                    TotalWorkouts = s.TotalWorkouts,
                    TotalCaloriesBurned = s.TotalCaloriesBurned,
                    UpdatedAt = s.UpdatedAt
                })
                .FirstOrDefaultAsync(cancellationToken);
            if (stats == null)
            {
                stats = new UserStatistic
                {
                    UserId = request.UserId,
                    TotalWorkouts = 1,
                    TotalCaloriesBurned = request.CaloriesBurned,
                    UpdatedAt = DateTime.UtcNow
                };
                await _statisticRepo.AddAsync(stats, cancellationToken);
            }
            else
            {
                stats.TotalWorkouts++;
                stats.TotalCaloriesBurned += request.CaloriesBurned;
                stats.UpdatedAt = DateTime.UtcNow;
                _statisticRepo.SaveInclude(stats, nameof(UserStatistic.TotalWorkouts),
                    nameof(UserStatistic.TotalCaloriesBurned), nameof(UserStatistic.UpdatedAt));
            }

            return Result.Success();
        }, cancellationToken);
    }
}