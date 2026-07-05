using FluentValidation;
using NutritionService.Features.GetMealRecommendations.Queries;

namespace NutritionService.Features.GetMealRecommendations.Vlidators
{
    public class GetMealRecommendationsQueryValidator : AbstractValidator<GetMealRecommendationsQuery>
    {
        public GetMealRecommendationsQueryValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");
            RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
            RuleFor(x => x.MaxCalories).GreaterThan(0).When(x => x.MaxCalories.HasValue);
            RuleFor(x => x.MinProtein).GreaterThanOrEqualTo(0).When(x => x.MinProtein.HasValue);
        }
    }
}
