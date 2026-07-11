using MediatR;
using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.Progress.Commands.LogWeight;

public class UpdateWeightStatisticCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
    public double WeightDifference { get; set; }
}

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
        return await _unitOfWork.ExecuteAsync(async() =>
        {
			var currentWeightLost = await _statisticRepo.Find(s => s.UserId == request.UserId)
		   .Select(s => (double?)s.TotalWeightLost)
		   .FirstOrDefaultAsync(cancellationToken);

			double newTotalWeightLost = 0;

			if (currentWeightLost.HasValue)
			{
				newTotalWeightLost = currentWeightLost.Value - request.WeightDifference;
				await _statisticRepo.Find(s => s.UserId == request.UserId)
					.ExecuteUpdateAsync(s => s
						.SetProperty(p => p.TotalWeightLost, newTotalWeightLost)
						.SetProperty(p => p.UpdatedAt, DateTime.UtcNow), cancellationToken);
			}
			else
			{
				newTotalWeightLost = request.WeightDifference < 0 ? Math.Abs(request.WeightDifference) : 0;
				var stats = new UserStatistic
				{
					UserId = request.UserId,
					TotalWorkouts = 0,
					TotalCaloriesBurned = 0,
					TotalWeightLost = newTotalWeightLost,
					UpdatedAt = DateTime.UtcNow
				};
				await _statisticRepo.AddAsync(stats, cancellationToken);
			}

			return Result.Success();

		},cancellationToken);
       
    }
}
