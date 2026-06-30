using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NutritionService.Common;
using NutritionService.Common.Database;
using NutritionService.Features.BrowseMealPlans.Dtos;
using NutritionService.Models;

namespace NutritionService.Features.BrowseMealPlans.Queries;

public record GetMealPlansByCaloriesQuery(int? Calories) : IRequest<Result<IEnumerable<MealPlanDto>>>;

public class GetMealPlansByCaloriesQueryHandler : IRequestHandler<GetMealPlansByCaloriesQuery, Result<IEnumerable<MealPlanDto>>>
{
    private readonly IGenericRepository<MealPlan> _mealPlanRepo;

    public GetMealPlansByCaloriesQueryHandler(IGenericRepository<MealPlan> mealPlanRepo)
    {
        _mealPlanRepo = mealPlanRepo;
    }

    public async Task<Result<IEnumerable<MealPlanDto>>> Handle(GetMealPlansByCaloriesQuery request, CancellationToken cancellationToken)
    {
        var mealPlans = await _mealPlanRepo.GetQueryable()
            .Where(mp => mp.TargetCalorieRangeMin <= request.Calories && mp.TargetCalorieRangeMax >= request.Calories)
            .OrderBy(mp => mp.TargetCalorieRangeMin)
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

        return Result<IEnumerable<MealPlanDto>>.Success(mealPlans);
    }
}
