using NutritionService.Common.Pagination;

namespace NutritionService.Features.GetMealRecommendations.Dtos;

public class MealRecommendationResponseDto
{
    public double UserDailyGoalCalories { get; init; }
    public PagedResponse<RecommendedMealDto>  RecommendedMeals { get; init; }
}
