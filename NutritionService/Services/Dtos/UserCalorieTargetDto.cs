namespace NutritionService.Services.Dtos;

public class UserCalorieTargetDto
{
    public int CalorieTarget { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsCalculated { get; set; }
}
