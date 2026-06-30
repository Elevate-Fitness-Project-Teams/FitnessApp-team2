namespace NutritionService.Models;

public class MealTag
{
    public Guid MealId { get; set; }
    public Guid TagId { get; set; }

    public Meal Meal { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
