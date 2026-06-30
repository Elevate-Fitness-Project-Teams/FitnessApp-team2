using MediatR;
using Microsoft.EntityFrameworkCore;
using NutritionService.Common;
using NutritionService.Common.Database;
using NutritionService.Common.Pagination;
using NutritionService.Features.BrowseMealPlans.Dtos;
using NutritionService.Models;

namespace NutritionService.Features.BrowseMealPlans.Queries;

public record BrowseMealPlansQuery(int PageNumber = 1, int PageSize = 10) : IRequest<Result<PagedResponse<MealPlanDto>>>;

public class BrowseMealPlansQueryHandler : IRequestHandler<BrowseMealPlansQuery, Result<PagedResponse<MealPlanDto>>>
{
    private readonly IGenericRepository<MealPlan> _mealPlanRepo;

    public BrowseMealPlansQueryHandler(IGenericRepository<MealPlan> mealPlanRepo)
    {
        _mealPlanRepo = mealPlanRepo;
    }

    public async Task<Result<PagedResponse<MealPlanDto>>> Handle(BrowseMealPlansQuery request, CancellationToken cancellationToken)
    {
        var query = _mealPlanRepo.GetQueryable();

        var totalCount = await query.CountAsync(cancellationToken);

        var pagedData = await query
            .OrderBy(mp => mp.Name) 
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(mp => new MealPlanDto
            {
                Id = mp.Id,
                Name = mp.Name,
                Description = mp.Description,
                TargetCalorieRangeMin = mp.TargetCalorieRangeMin,
                TargetCalorieRangeMax = mp.TargetCalorieRangeMax,
                Schedule = mp.MealPlanItems.Select(mpi => new MealPlanItemDto
                {
                    Id = mpi.Id,
                    MealId = mpi.MealId,
                    MealName = mpi.Meal.Name,
                    DayOfWeek = mpi.DayOfWeek,
                    MealTime = mpi.MealTime
                })
            })
            .ToListAsync(cancellationToken);

        var pagedResponse = new PagedResponse<MealPlanDto>(pagedData, totalCount, request.PageNumber, request.PageSize);

        return Result<PagedResponse<MealPlanDto>>.Success(pagedResponse);
    }
}
