using MediatR;
using Microsoft.EntityFrameworkCore;
using NutritionService.Common;
using NutritionService.Common.Database;
using NutritionService.Common.Errors;
using NutritionService.Common.Pagination;
using NutritionService.Features.GetMealRecommendations.Dtos;
using NutritionService.Models;
using NutritionService.Services;

namespace NutritionService.Features.GetMealRecommendations.Queries;

public class GetMealRecommendationsQueryHandler : IRequestHandler<GetMealRecommendationsQuery, Result<MealRecommendationResponseDto>>
{
    private readonly IGenericRepository<Meal> _mealRepo;
    private readonly IFceHttpClient _fceClient;

    public GetMealRecommendationsQueryHandler(IGenericRepository<Meal> mealRepo, IFceHttpClient fceClient)
    {
        _mealRepo = mealRepo;
        _fceClient = fceClient;
    }

    public async Task<Result<MealRecommendationResponseDto>> Handle(GetMealRecommendationsQuery request, CancellationToken cancellationToken)
    {
        var fceResult = await _fceClient.GetUserMetricsAsync(Guid.Parse(request.UserId), cancellationToken);

        if (fceResult is null)
        {
            return Result<MealRecommendationResponseDto>.Failure(
                Error.Failure("FCE_METRICS_NOT_CALCULATED", NutritionErrors.FceMetricsNotCalculated));
        }

        var calorieTarget = fceResult.CalorieTarget;

      
        var query = _mealRepo.GetQueryable();

        if (!string.IsNullOrWhiteSpace(request.MealType))
        {
            query = query.Where(m => m.MealType == request.MealType);
        }

        
        var effectiveMaxCalories = request.MaxCalories ?? calorieTarget;
        query = query.Where(m => m.Calories <= effectiveMaxCalories);

        if (request.MinProtein.HasValue)
        {
            query = query.Where(m => m.Protein >= request.MinProtein.Value);
        }
       
        var totalRecords = await query.CountAsync(cancellationToken);

        var meals = await query
            .OrderBy(m => m.Calories)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(m => new RecommendedMealDto
            {
                Id = m.Id,
                Name = m.Name,
                Difficulty = m.Difficulty,
                MealType = m.MealType,
                PrepTimeInMinutes = m.PrepTimeInMinutes,
                ImageUrl = m.ImageUrl,
                Calories = m.Calories,
                Carbs = m.Carbs,
                Protein = m.Protein,
                Fats = m.Fats,
                Fiber = m.Fiber,
            })
            .ToListAsync(cancellationToken);

        var pagedMeals = new PagedResponse<RecommendedMealDto>(meals, totalRecords, request.Page, request.PageSize);

        var responseData = new MealRecommendationResponseDto
        {
            UserDailyGoalCalories = calorieTarget,
            RecommendedMeals = pagedMeals
        };

        return Result<MealRecommendationResponseDto>.Success(responseData);
    }
}
