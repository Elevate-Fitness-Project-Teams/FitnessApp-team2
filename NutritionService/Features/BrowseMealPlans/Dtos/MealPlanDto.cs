namespace NutritionService.Features.BrowseMealPlans.Dtos;

public class MealPlanDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int TargetCalorieRangeMin { get; init; }
    public int TargetCalorieRangeMax { get; init; }
    
    public IEnumerable<MealPlanItemDto> Schedule { get; init; } = Enumerable.Empty<MealPlanItemDto>();
}
