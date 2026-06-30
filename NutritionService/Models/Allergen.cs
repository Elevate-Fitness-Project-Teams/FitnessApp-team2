namespace NutritionService.Models;

public class Allergen : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<MealAllergen> MealAllergens { get; set; } = new List<MealAllergen>();
}
