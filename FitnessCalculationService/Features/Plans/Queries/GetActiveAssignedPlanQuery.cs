using FitnessCalculationService.Common;
using FitnessCalculationService.Domain.Entities;
using FitnessCalculationService.Features.Plans.DTOs;
using FitnessCalculationService.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitnessCalculationService.Features.Plans.Queries;

public record GetActiveAssignedPlanQuery(Guid UserId) : IRequest<Result<ActivePlanDto?>>;

public class GetActiveAssignedPlanQueryHandler : IRequestHandler<GetActiveAssignedPlanQuery, Result<ActivePlanDto?>>
{
    private readonly IGenericRepository<UserAssignedPlan> _assignedRepo;

    public GetActiveAssignedPlanQueryHandler(IGenericRepository<UserAssignedPlan> assignedRepo)
    {
        _assignedRepo = assignedRepo;
    }

    public async Task<Result<ActivePlanDto?>> Handle(GetActiveAssignedPlanQuery request, CancellationToken cancellationToken)
    {
        var activePlan = await _assignedRepo.GetQueryable()
            .AsNoTracking()
            .Where(p => p.UserId == request.UserId && p.IsActive)
            .Select(p => new ActivePlanDto(
                p.Id,
                p.FitnessPlanConfigId,
                p.FitnessPlanConfig.PlanId,
                p.AssignedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return Result<ActivePlanDto?>.Success(activePlan);
    }
}
