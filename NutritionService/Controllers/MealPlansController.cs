using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutritionService.Common;
using NutritionService.Common.Pagination;
using NutritionService.Features.BrowseMealPlans.Dtos;
using NutritionService.Features.BrowseMealPlans.Queries;
using NutritionService.Features.GetMealDetail.Dtos;
using NutritionService.Features.GetMealDetail.Queries;
using NutritionService.Features.GetMealRecommendations.Dtos;
using NutritionService.Features.GetMealRecommendations.Queries;

namespace NutritionService.Controllers;

[ApiController]
[Route("api/v1/nutrition")]
[Authorize]
public class MealPlansController : ControllerBase
{
    private readonly IMediator _mediator;

    public MealPlansController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [HttpGet("meals/{id:guid}")]
    public async Task<IActionResult> GetMealDetail(Guid id)
    {
        var query = new GetMealDetailsQuery(id);
        var result = await _mediator.Send(query);

        if (result.IsSuccess)
        {
            return Ok(ApiResponse<MealDetailDto>.Success(result.Value));
        }

        var errorMessages = result.Errors.Select(e => e.Description);

        if (result.Errors.Any(e => e.Code == "MealNotFound" || e.Code == "RES_MEAL_NOT_FOUND"))
        {
            return NotFound(ApiResponse<MealDetailDto>.Failure(errorMessages, "Not Found", 404));
        }
        return BadRequest(ApiResponse<MealDetailDto>.Failure(errorMessages, "Validation Failed", 400));
    }

    [HttpGet("meal-plans")]
    public async Task<IActionResult> BrowseMealPlans([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var query = new BrowseMealPlansQuery(pageNumber, pageSize);
        var result = await _mediator.Send(query);

        if (result.IsSuccess)
        {
            return Ok(ApiResponse<PagedResponse<MealPlanDto>>.Success(result.Value));
        }

        return BadRequest(ApiResponse<PagedResponse<MealPlanDto>>.Failure(result.Errors.Select(e => e.Description)));
    }

    [HttpGet("meal-plans/by-calories")]
    public async Task<IActionResult> GetMealPlansByCalories([FromQuery] int? calories)
    {
        var query = new GetMealPlansByCaloriesQuery(calories);
        var result = await _mediator.Send(query);

        if (result.IsSuccess)
        {
            return Ok(ApiResponse<IEnumerable<MealPlanDto>>.Success(result.Value));
        }
       
        var errorMessages = result.Errors.Select(e => e.Description);
        
        if (result.Errors.Any(e => e.Code == "VAL_REQUIRED_FIELD"))
        {
            return BadRequest(ApiResponse<IEnumerable<MealPlanDto>>.Failure(errorMessages, "Validation Failed", 400));
        }

        return BadRequest(ApiResponse<IEnumerable<MealPlanDto>>.Failure(errorMessages));
    }
    [HttpGet("recommendations")]
    public async Task<IActionResult> GetRecommendations(
        [FromQuery] string? mealType,
        [FromQuery] int? maxCalories,
        [FromQuery] double? minProtein,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<MealRecommendationResponseDto>.Failure(new[] { "User ID claim is missing." }, "Unauthorized", 401));
        }

        var query = new GetMealRecommendationsQuery(userId, mealType, maxCalories, minProtein, page, pageSize);
        var result = await _mediator.Send(query);

        if (result.IsSuccess)
        {
            return Ok(ApiResponse<MealRecommendationResponseDto>.Success(result.Value));
        }

        var errorMessages = result.Errors.Select(e => e.Description);

        if (result.Errors.Any(e => e.Code == "FCE_METRICS_NOT_CALCULATED"))
        {
            return BadRequest(ApiResponse<MealRecommendationResponseDto>.Failure(errorMessages, "FCE Metrics Not Calculated", 400));
        }

        return BadRequest(ApiResponse<MealRecommendationResponseDto>.Failure(errorMessages));
    }
    [HttpGet("recommendations/{userId}")]
    public async Task<IActionResult> GetRecommendationsByUserId(
       string userId,
       [FromQuery] string? mealType,
       [FromQuery] int? maxCalories,
       [FromQuery] double? minProtein,
       [FromQuery] int page = 1,
       [FromQuery] int pageSize = 10)
    {
        var query = new GetMealRecommendationsQuery(userId, mealType, maxCalories, minProtein, page, pageSize);
        var result = await _mediator.Send(query);

        if (result.IsSuccess)
        {
            return Ok(ApiResponse<MealRecommendationResponseDto>.Success(result.Value));
        }

        var errorMessages = result.Errors.Select(e => e.Description);

        if (result.Errors.Any(e => e.Code == "FCE_METRICS_NOT_CALCULATED"))
        {
            return BadRequest(ApiResponse<MealRecommendationResponseDto>.Failure(errorMessages, "FCE Metrics Not Calculated", 400));
        }

        return BadRequest(ApiResponse<MealRecommendationResponseDto>.Failure(errorMessages));
    }
}
