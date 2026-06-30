namespace NutritionService.Models;

public class Ingredient : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    // Navigation property
    public ICollection<MealIngredient> MealIngredients { get; set; } = new List<MealIngredient>();
}
