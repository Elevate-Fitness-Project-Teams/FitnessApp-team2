using FitnessCalculationService.Common;
using FitnessCalculationService.Common.Paging;
using FitnessCalculationService.Domain.Entities;
using FitnessCalculationService.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitnessCalculationService.Features.Plans.Queries.GetPlanHistory;

public record GetPlanHistoryQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<PlanHistoryItemResponse>>>;

public class GetPlanHistoryQueryHandler : IRequestHandler<GetPlanHistoryQuery, Result<PagedResult<PlanHistoryItemResponse>>>
{
    private readonly IGenericRepository<UserPlanHistory> _historyRepo;

    public GetPlanHistoryQueryHandler(IGenericRepository<UserPlanHistory> historyRepo)
    {
        _historyRepo = historyRepo;
    }

    public async Task<Result<PagedResult<PlanHistoryItemResponse>>> Handle(GetPlanHistoryQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var query = _historyRepo.GetQueryable()
            .AsNoTracking()
            .Where(h => h.UserId == request.UserId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(h => h.AssignedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(h => new PlanHistoryItemResponse(
                h.Id,
                h.PlanId,
                h.PreviousPlanId,
                h.Goal.ToString(),
                h.Status.ToString(),
                h.PreviousStatus != null ? h.PreviousStatus.ToString() : null,
                h.Bmr,
                h.Tdee,
                h.CalorieTarget,
                h.AssignedPlanId,
                h.TriggeredBy,
                h.Reason,
                h.AssignedAt
            ))
            .ToListAsync(cancellationToken);

        var pagedResult = PagedResult<PlanHistoryItemResponse>.Create(items, page, pageSize, totalCount);
        return Result<PagedResult<PlanHistoryItemResponse>>.Success(pagedResult);
    }
}
