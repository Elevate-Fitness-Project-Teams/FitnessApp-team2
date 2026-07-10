namespace NutritionService.Features.GetMealRecommendations.Dtos;

public class RecommendedMealDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string MealType { get; init; } = string.Empty;
    public int PrepTimeInMinutes { get; init; }
    public string Difficulty { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public int Calories { get; set; }
    public double Protein { get; set; }
    public double Carbs { get; set; }
    public double Fats { get; set; }
    public double Fiber { get; set; }
}
