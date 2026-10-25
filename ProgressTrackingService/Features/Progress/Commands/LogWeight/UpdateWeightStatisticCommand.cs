using MediatR;
using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.Progress.Commands.LogWeight;

public record UpdateWeightStatisticCommand(Guid UserId, double InitialWeight, double CurrentWeight) : IRequest<Result>;

public class UpdateWeightStatisticHandler : IRequestHandler<UpdateWeightStatisticCommand, Result>
{
    private readonly IGeneralRepo<UserStatistic> _statisticRepo;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateWeightStatisticHandler(IGeneralRepo<UserStatistic> statisticRepo, IUnitOfWork unitOfWork)
    {
        _statisticRepo = statisticRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateWeightStatisticCommand request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            var totalLost = Math.Max(0.0, Math.Round(request.InitialWeight - request.CurrentWeight, 2));

            var updated = await _statisticRepo.Find(s => s.UserId == request.UserId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.InitialWeight, request.InitialWeight)
                    .SetProperty(p => p.CurrentWeight, request.CurrentWeight)
                    .SetProperty(p => p.TotalWeightLost, totalLost)
                    .SetProperty(p => p.UpdatedAt, DateTime.UtcNow), cancellationToken);

            if (updated == 0)
            {
                var stats = new UserStatistic
                {
                    UserId = request.UserId,
                    TotalWorkouts = 0,
                    TotalCaloriesBurned = 0,
                    TotalMinutes = 0,
                    InitialWeight = request.InitialWeight,
                    CurrentWeight = request.CurrentWeight,
                    TotalWeightLost = totalLost,
                    UpdatedAt = DateTime.UtcNow
                };
                await _statisticRepo.AddAsync(stats, cancellationToken);
            }

            return Result.Success();
        }, cancellationToken);
    }
}
