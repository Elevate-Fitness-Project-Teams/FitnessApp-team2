using FluentValidation;
using NutritionService.Features.BrowseMealPlans.Queries;

namespace NutritionService.Features.BrowseMealPlans.Validators
{
    public class GetMealPlansByCaloriesQueryValidator : AbstractValidator<GetMealPlansByCaloriesQuery>
    {
        public GetMealPlansByCaloriesQueryValidator()
        {
            RuleFor(x => x.Calories)
                .NotNull()
                .WithErrorCode("VAL_REQUIRED_FIELD")
                .WithMessage("Calories parameter is required.");
        }
    }
}
