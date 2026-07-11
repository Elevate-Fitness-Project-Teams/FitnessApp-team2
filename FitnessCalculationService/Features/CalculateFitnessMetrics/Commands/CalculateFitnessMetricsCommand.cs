using FitnessCalculationService.Common;
using FitnessCalculationService.Domain.Entities;
using FitnessCalculationService.Features.CalculateFitnessMetrics.Response;
using FitnessCalculationService.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitnessCalculationService.Features.CalculateFitnessMetrics.Commands
{
    public record CalculateFitnessMetricsCommand(string UserId, double Bmr, double Tdee, double CalorieTarget, string Status) : IRequest<Result>;

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
                        CalculatedAt = DateTime.UtcNow
                    };
                    await _metricsRepo.AddAsync(newMetrics, cancellationToken);
                }

                return Result.Success();
            });
        }    
    }
}
