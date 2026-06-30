namespace NutritionService.Models;

public class MealAllergen
{
    public Guid MealId { get; set; }
    public Guid AllergenId { get; set; }

    public Meal Meal { get; set; } = null!;
    public Allergen Allergen { get; set; } = null!;
}
