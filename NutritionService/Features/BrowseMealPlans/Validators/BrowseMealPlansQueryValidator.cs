using FluentValidation;
using NutritionService.Features.BrowseMealPlans.Queries;

namespace NutritionService.Features.BrowseMealPlans.Validators
{
    public class BrowseMealPlansQueryValidator : AbstractValidator<BrowseMealPlansQuery>
    {
        public BrowseMealPlansQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Page number must be at least 1.");
            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 50)
                .WithMessage("Page size must be between 1 and 50.");
        }
    }
}
