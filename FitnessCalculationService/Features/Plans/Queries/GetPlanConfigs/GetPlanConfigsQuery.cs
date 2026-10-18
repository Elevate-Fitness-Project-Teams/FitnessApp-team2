using FitnessCalculationService.Common;
using FitnessCalculationService.Common.Paging;
using FitnessCalculationService.Domain.Entities;
using FitnessCalculationService.Domain.Enums;
using FitnessCalculationService.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitnessCalculationService.Features.Plans.Queries.GetPlanConfigs;

public record GetPlanConfigsQuery(
    string? Goal = null,
    string? Status = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<PlanConfigListItem>>>;

public class GetPlanConfigsQueryHandler : IRequestHandler<GetPlanConfigsQuery, Result<PagedResult<PlanConfigListItem>>>
{
    private readonly IGenericRepository<FitnessPlanConfig> _planRepo;

    public GetPlanConfigsQueryHandler(IGenericRepository<FitnessPlanConfig> planRepo)
    {
        _planRepo = planRepo;
    }

    public async Task<Result<PagedResult<PlanConfigListItem>>> Handle(GetPlanConfigsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var query = _planRepo.GetQueryable()
            .AsNoTracking()
            .Where(p => p.IsActive);

        // Optional Goal filter
        if (!string.IsNullOrWhiteSpace(request.Goal) && Enum.TryParse<FitnessGoal>(request.Goal.Replace(" ", ""), true, out var parsedGoal))
        {
            query = query.Where(p => p.Goal == parsedGoal);
        }

        // Optional Status filter
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<CalorieStatus>(request.Status, true, out var parsedStatus))
        {
            query = query.Where(p => p.Status == parsedStatus);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(p => p.Goal)
            .ThenBy(p => p.Status)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PlanConfigListItem(
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
            .ToListAsync(cancellationToken);

        var pagedResult = PagedResult<PlanConfigListItem>.Create(items, page, pageSize, totalCount);
        return Result<PagedResult<PlanConfigListItem>>.Success(pagedResult);
    }
}
