using FitnessCalculationService.Common;
using FitnessCalculationService.Common.Errors;
using FitnessCalculationService.Domain.Entities;
using FitnessCalculationService.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitnessCalculationService.Features.Plans.Queries.GetPlanById;

public record GetPlanByIdQuery(string PlanId) : IRequest<Result<PlanDetailResponse>>;

public class GetPlanByIdQueryHandler : IRequestHandler<GetPlanByIdQuery, Result<PlanDetailResponse>>
{
    private readonly IGenericRepository<FitnessPlanConfig> _planRepo;
    private readonly FitnessCalculationService.Common.Caching.ICacheService _cache;

    public GetPlanByIdQueryHandler(
        IGenericRepository<FitnessPlanConfig> planRepo,
        FitnessCalculationService.Common.Caching.ICacheService cache)
    {
        _planRepo = planRepo;
        _cache = cache;
    }

    public async Task<Result<PlanDetailResponse>> Handle(GetPlanByIdQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.PlanId))
        {
            return Result<PlanDetailResponse>.Failure(new Error(ErrorCodes.ValRequiredField, "PlanId is required."));
        }

        var cacheKey = $"plancfg:id:{request.PlanId.ToUpperInvariant()}";
        var cached = await _cache.GetAsync<PlanDetailResponse>(cacheKey, cancellationToken);
        if (cached != null)
        {
            return Result<PlanDetailResponse>.Success(cached);
        }

        var plan = await _planRepo.GetQueryable()
            .AsNoTracking()
            .Where(p => p.PlanId == request.PlanId)
            .Select(p => new PlanDetailResponse(
                p.Id,
                p.PlanId,
                p.Name,
                p.Description,
                p.Goal.ToString(),
                p.Status.ToString(),
                p.MinCalorie,
                p.MaxCalorie,
                p.EstimatedDurationWeeks,
                p.WorkoutsPerWeek,
                p.ProgramType,
                p.IsActive
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (plan == null)
        {
            return Result<PlanDetailResponse>.Failure(new Error(ErrorCodes.ResPlanNotFound, $"Fitness plan with ID '{request.PlanId}' was not found."));
        }

        await _cache.SetAsync(cacheKey, plan, TimeSpan.FromHours(24), cancellationToken);
        return Result<PlanDetailResponse>.Success(plan);
    }
}
