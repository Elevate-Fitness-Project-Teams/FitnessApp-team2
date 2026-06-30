using FluentValidation;
using NutritionService.Features.GetMealDetail.Queries;

namespace NutritionService.Features.GetMealDetail.Validators
{
    public class GetMealDetailsQueryValidator : AbstractValidator<GetMealDetailsQuery>
    {
        public GetMealDetailsQueryValidator()
        {
            RuleFor(x => x.MealId)
                .NotEmpty()
                .WithErrorCode("VAL_INVALID_ID")
                .WithMessage("A valid Meal ID is required.");
        }
    }
}
