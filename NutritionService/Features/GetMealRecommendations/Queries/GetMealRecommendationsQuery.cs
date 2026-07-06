using FluentValidation;
using MediatR;
using NutritionService.Common;
using NutritionService.Common.Pagination;
using NutritionService.Features.GetMealRecommendations.Dtos;

namespace NutritionService.Features.GetMealRecommendations.Queries;

public record GetMealRecommendationsQuery(
    string UserId,
    string? MealType,
    int? MaxCalories,
    double? MinProtein,
    int Page = 1,
    int PageSize = 10
) : IRequest<Result<MealRecommendationResponseDto>>;


