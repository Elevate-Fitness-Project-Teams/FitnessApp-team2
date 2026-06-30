namespace NutritionService.Models;

public class Meal : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string MealType { get; set; } = string.Empty;
    public int PrepTimeInMinutes { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string InstructionsJson { get; set; } = string.Empty;
    public string? VariationsJson { get; set; }
    
    // Flattened Nutrition Facts
    public int Calories { get; set; }
    public double Protein { get; set; }
    public double Carbs { get; set; }
    public double Fats { get; set; }
    public double Fiber { get; set; }

    // Navigation properties
    public ICollection<MealIngredient> MealIngredients { get; set; } = new List<MealIngredient>();
    public ICollection<MealPlanItem> MealPlanItems { get; set; } = new List<MealPlanItem>();
    public ICollection<MealTag> MealTags { get; set; } = new List<MealTag>();
    public ICollection<MealAllergen> MealAllergens { get; set; } = new List<MealAllergen>();
}
