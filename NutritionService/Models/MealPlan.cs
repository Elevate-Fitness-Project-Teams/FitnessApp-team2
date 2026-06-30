namespace NutritionService.Models;

public class MealPlan : BaseEntity
{

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TargetCalorieRangeMin { get; set; }
    public int TargetCalorieRangeMax { get; set; }

    // Navigation properties
    public ICollection<MealPlanItem> MealPlanItems { get; set; } = new List<MealPlanItem>();
}
