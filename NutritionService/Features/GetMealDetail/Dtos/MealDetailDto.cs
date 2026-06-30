namespace NutritionService.Features.GetMealDetail.Dtos
{
    public class MealDetailDto
    {
        public string Name { get; init; } = string.Empty;
        public string MealType { get; init; } = string.Empty;
        public int PrepTimeInMinutes { get; init; }
        public string Difficulty { get; init; } = string.Empty;
        public string ImageUrl { get; init; } = string.Empty;

       
        public IEnumerable<string> Ingredients { get; init; } = [];
        public IEnumerable<string> Instructions { get; init; } = [];
        public IEnumerable<string>? Variations { get; init; }
        public IEnumerable<string> Allergens { get; init; } = [];
        public IEnumerable<string> Tags { get; init; } = [];

     
        public NutritionFactsDto Nutrition { get; init; } = null!;
    }
}
