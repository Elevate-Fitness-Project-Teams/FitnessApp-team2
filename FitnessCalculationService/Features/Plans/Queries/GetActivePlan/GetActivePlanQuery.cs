using FitnessCalculationService.Common;
using FitnessCalculationService.Common.Errors;
using FitnessCalculationService.Domain.Entities;
using FitnessCalculationService.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitnessCalculationService.Features.Plans.Queries.GetActivePlan;

public record GetActivePlanQuery(Guid UserId) : IRequest<Result<ActivePlanDetailResponse>>;

public class GetActivePlanQueryHandler : IRequestHandler<GetActivePlanQuery, Result<ActivePlanDetailResponse>>
{
    private readonly IGenericRepository<UserAssignedPlan> _assignedRepo;

    public GetActivePlanQueryHandler(IGenericRepository<UserAssignedPlan> assignedRepo)
    {
        _assignedRepo = assignedRepo;
    }

    public async Task<Result<ActivePlanDetailResponse>> Handle(GetActivePlanQuery request, CancellationToken cancellationToken)
    {
        var activePlan = await _assignedRepo.GetQueryable()
            .AsNoTracking()
            .Where(p => p.UserId == request.UserId && p.IsActive)
            .Select(p => new ActivePlanDetailResponse(
                p.Id,
                p.FitnessPlanConfigId,
                p.FitnessPlanConfig.PlanId,
                p.FitnessPlanConfig.Name,
                p.FitnessPlanConfig.Description,
                p.FitnessPlanConfig.Goal.ToString(),
                p.FitnessPlanConfig.Status.ToString(),
                p.FitnessPlanConfig.EstimatedDurationWeeks,
                p.FitnessPlanConfig.WorkoutsPerWeek,
                p.FitnessPlanConfig.ProgramType,
                p.FitnessPlanConfig.MinCalorie,
                p.FitnessPlanConfig.MaxCalorie,
                p.AssignedAt,
                p.IsActive
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (activePlan == null)
        {
            return Result<ActivePlanDetailResponse>.Failure(
                new Error(ErrorCodes.FceNoMatchingPlan, $"No active assigned plan found for user {request.UserId}."));
        }

        return Result<ActivePlanDetailResponse>.Success(activePlan);
    }
}
