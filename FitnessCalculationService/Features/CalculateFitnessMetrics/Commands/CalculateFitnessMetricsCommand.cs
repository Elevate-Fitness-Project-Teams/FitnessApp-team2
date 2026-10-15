using FitnessCalculationService.Common;
using FitnessCalculationService.Domain.Entities;
using FitnessCalculationService.Domain.Enums;
using FitnessCalculationService.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitnessCalculationService.Features.CalculateFitnessMetrics.Commands
{
    public record CalculateFitnessMetricsCommand(
        Guid UserId, 
        double Bmr, 
        double Tdee, 
        double CalorieTarget, 
        CalorieStatus Status,
        Guid? SourceStatsId = null
    ) : IRequest<Result>;

    public class CalculateFitnessMetricsCommandHandler : IRequestHandler<CalculateFitnessMetricsCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<CalculatedMetrics> _metricsRepo;

        public CalculateFitnessMetricsCommandHandler(IUnitOfWork unitOfWork, IGenericRepository<CalculatedMetrics> metricsRepo)
        {
            _unitOfWork = unitOfWork;
            _metricsRepo = metricsRepo;
        }

        public async Task<Result> Handle(CalculateFitnessMetricsCommand request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.ExecuteAsync(async () =>
            {
                var rowsAffected = await _metricsRepo.GetQueryable()
                    .Where(x => x.UserId == request.UserId)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(m => m.Bmr, request.Bmr)
                        .SetProperty(m => m.Tdee, request.Tdee)
                        .SetProperty(m => m.CalorieTarget, request.CalorieTarget)
                        .SetProperty(m => m.Status, request.Status)
                        .SetProperty(m => m.SourceStatsId, request.SourceStatsId)
                        .SetProperty(m => m.CalculatedAt, DateTime.UtcNow),
                        cancellationToken);

                if (rowsAffected == 0)
                {
                    var newMetrics = new CalculatedMetrics
                    {
                        Id = Guid.CreateVersion7(),
                        UserId = request.UserId,
                        Bmr = request.Bmr,
                        Tdee = request.Tdee,
                        CalorieTarget = request.CalorieTarget,
                        Status = request.Status,
                        SourceStatsId = request.SourceStatsId,
                        CalculatedAt = DateTime.UtcNow
                    };
                    await _metricsRepo.AddAsync(newMetrics, cancellationToken);
                }

                return Result.Success();
            }, cancellationToken);
        }    
    }
}
