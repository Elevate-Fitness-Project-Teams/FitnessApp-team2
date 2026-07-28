using FitnessCalculationService.Common;
using FitnessCalculationService.Domain.Entities;
using FitnessCalculationService.Domain.Services;
using FitnessCalculationService.Features.CalculateFitnessMetrics.Response;
using FitnessCalculationService.Features.FitnessStats.Queries.GetFitnessStats;
using FitnessCalculationService.Persistence;
using FitnessCalculationService.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitnessCalculationService.Features.CalculateFitnessMetrics.Commands
{
    public record CalculateFitnessMetricsOrchstrator(string UserId) : IRequest<Result<CalculateFitnessMetricsResponse>>;

    public class CalculateFitnessMetricsOrchstratorHandler : IRequestHandler<CalculateFitnessMetricsOrchstrator, Result<CalculateFitnessMetricsResponse>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IGenericRepository<UserFitnessStats> _statsRepo;
        private readonly IGenericRepository<CalculatedMetrics> _metricsRepo;
        private readonly IMetabolicCalculator _calculator;
        private readonly IMediator _mediator;

        public CalculateFitnessMetricsOrchstratorHandler(
            IUnitOfWork uow, 
            IMetabolicCalculator calculator,
            IMediator mediator)
        {
            _uow = uow;
            _calculator = calculator;
            _mediator = mediator;
        }

        public async Task<Result<CalculateFitnessMetricsResponse>> Handle(CalculateFitnessMetricsOrchstrator request, CancellationToken cancellationToken)
        {
            return await _uow.ExecuteAsync(async () =>
            {
                var stats = await _mediator.Send(new GetFitnessStatsQuery(request.UserId), cancellationToken);

                if (stats == null)
                {
                    return Result<CalculateFitnessMetricsResponse>.Failure(new Error("FCE_STATS_NOT_FOUND", "No UserFitnessStats row has been recorded yet."));
                }

                var genderEnum = Enum.Parse<FitnessCalculationService.Domain.Enums.Gender>(stats.Value.Gender, true);
                var activityLevelEnum = Enum.Parse<FitnessCalculationService.Domain.Enums.ActivityLevel>(stats.Value.ActivityLevel, true);
                var goalEnum = Enum.Parse<FitnessCalculationService.Domain.Enums.FitnessGoal>(stats.Value.Goal, true);

                var bmr = _calculator.CalculateBmr(stats.Value.Weight, stats.Value.Height, stats.Value.Age, genderEnum);
                var tdee = _calculator.CalculateTdee(bmr, activityLevelEnum);
                var calorieTarget = _calculator.CalculateCalorieTarget(tdee, goalEnum);
                var status = _calculator.ClassifyStatus(calorieTarget);

                if (!double.IsFinite(bmr) || !double.IsFinite(tdee) || !double.IsFinite(calorieTarget))
                {
                    return Result<CalculateFitnessMetricsResponse>.Failure(new Error("FCE_INVALID_CALCULATION", "Calculation produces a non-finite or invalid numeric result."));
                }

              var updateOrAddMetrics = await _mediator.Send(new CalculateFitnessMetricsCommand(request.UserId, bmr, tdee, calorieTarget, status.ToString()), cancellationToken);
              if(!updateOrAddMetrics.IsSuccess)
              {
                  return Result<CalculateFitnessMetricsResponse>.Failure(new Error("FCE_METRICS_UPDATE_FAILED", "Failed to update or add calculated metrics."));
              }

                var response = new CalculateFitnessMetricsResponse(bmr, tdee, calorieTarget, status.ToString());

                return Result<CalculateFitnessMetricsResponse>.Success(response);

            }, cancellationToken);
        }
    }
}
