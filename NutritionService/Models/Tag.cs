namespace NutritionService.Models;

public class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<MealTag> MealTags { get; set; } = new List<MealTag>();
}
