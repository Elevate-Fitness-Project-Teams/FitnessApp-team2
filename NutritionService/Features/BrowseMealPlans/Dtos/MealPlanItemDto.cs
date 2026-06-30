namespace NutritionService.Features.BrowseMealPlans.Dtos;

public class MealPlanItemDto
{
    public Guid Id { get; init; }
    public Guid MealId { get; init; }
    public string MealName { get; init; } = string.Empty;
    public string DayOfWeek { get; init; } = string.Empty;
    public string MealTime { get; init; } = string.Empty;
}
