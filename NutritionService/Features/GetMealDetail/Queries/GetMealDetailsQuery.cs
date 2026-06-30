using MediatR;
using Microsoft.EntityFrameworkCore;
using NutritionService.Common;
using NutritionService.Common.Database;
using NutritionService.Common.Helpers;
using NutritionService.Features.GetMealDetail.Dtos;
using NutritionService.Models;

namespace NutritionService.Features.GetMealDetail.Queries
{
    public record GetMealDetailsQuery(Guid MealId) : IRequest<Result<MealDetailDto>>;

    public class GetMealDetailsQueryHandler : IRequestHandler<GetMealDetailsQuery, Result<MealDetailDto>>
    {
        private readonly IGenericRepository<Meal> _mealRepo;

        public GetMealDetailsQueryHandler(IGenericRepository<Meal> mealRepo)
        {
            _mealRepo = mealRepo;
        }

        public async Task<Result<MealDetailDto>> Handle(GetMealDetailsQuery request, CancellationToken cancellationToken)
        {
            var rawMealData = await _mealRepo.GetQueryable()
               .Where(m => m.Id == request.MealId)
               .Select(m => new
               {
                   m.Name,
                   m.MealType,
                   m.PrepTimeInMinutes,
                   m.Difficulty,
                   m.ImageUrl,
                   m.InstructionsJson,
                   m.VariationsJson,
                   Ingredients = m.MealIngredients.Select(mi => mi.Amount + " " + mi.Ingredient.Name).ToList(),
                   Tags = m.MealTags.Select(mt => mt.Tag.Name).ToList(),
                   Allergens = m.MealAllergens.Select(ma => ma.Allergen.Name).ToList(),
                   Nutrition = new NutritionFactsDto
                   {
                       Calories = m.Calories,
                       Protein = m.Protein,
                       Carbs = m.Carbs,
                       Fats = m.Fats,
                       Fiber = m.Fiber
                   }
               })
               .FirstOrDefaultAsync(cancellationToken);
               
            if (rawMealData == null)
                return Result<MealDetailDto>.Failure(Error.NotFound("RES_MEAL_NOT_FOUND", "The specified meal was not found."));

            var response = new MealDetailDto
            {
                Name = rawMealData.Name,
                MealType = rawMealData.MealType,
                PrepTimeInMinutes = rawMealData.PrepTimeInMinutes,
                Difficulty = rawMealData.Difficulty,
                ImageUrl = rawMealData.ImageUrl,            
                Ingredients = rawMealData.Ingredients,
                Instructions = rawMealData.InstructionsJson.ToParsedStringArray(),
                Variations = rawMealData.VariationsJson.ToParsedStringArray(),
                Allergens = rawMealData.Allergens,
                Tags = rawMealData.Tags,
                Nutrition = rawMealData.Nutrition
            };
            return Result<MealDetailDto>.Success(response);
        }
    }
}