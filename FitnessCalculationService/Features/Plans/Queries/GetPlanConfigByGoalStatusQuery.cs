using FitnessCalculationService.Common;
using FitnessCalculationService.Common.Errors;
using FitnessCalculationService.Domain.Entities;
using FitnessCalculationService.Domain.Enums;
using FitnessCalculationService.Features.Plans.DTOs;
using FitnessCalculationService.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitnessCalculationService.Features.Plans.Queries;

public record GetPlanConfigByGoalStatusQuery(FitnessGoal Goal, CalorieStatus Status) : IRequest<Result<PlanConfigDto>>;

public class GetPlanConfigByGoalStatusQueryHandler : IRequestHandler<GetPlanConfigByGoalStatusQuery, Result<PlanConfigDto>>
{
    private readonly IGenericRepository<FitnessPlanConfig> _planRepo;

    public GetPlanConfigByGoalStatusQueryHandler(IGenericRepository<FitnessPlanConfig> planRepo)
    {
        _planRepo = planRepo;
    }

    public async Task<Result<PlanConfigDto>> Handle(GetPlanConfigByGoalStatusQuery request, CancellationToken cancellationToken)
    {
        var plan = await _planRepo.GetQueryable()
            .AsNoTracking()
            .Where(p => p.Goal == request.Goal && p.Status == request.Status && p.IsActive)
            .Select(p => new PlanConfigDto(
                p.Id,
                p.PlanId,
                p.Name,
                p.Description,
                p.Goal,
                p.Status,
                p.MinCalorie,
                p.MaxCalorie,
                p.EstimatedDurationWeeks,
                p.WorkoutsPerWeek,
                p.ProgramType
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (plan == null)
        {
            return Result<PlanConfigDto>.Failure(new Error(ErrorCodes.FceNoMatchingPlan, $"No active fitness plan found matching goal '{request.Goal}' and status '{request.Status}'."));
        }

        return Result<PlanConfigDto>.Success(plan);
    }
}
