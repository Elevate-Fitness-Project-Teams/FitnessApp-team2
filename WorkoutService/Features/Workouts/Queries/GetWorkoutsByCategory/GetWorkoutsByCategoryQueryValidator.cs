using FluentValidation;

namespace WorkoutService.Features.Workouts.Queries.GetWorkoutsByCategory;

public class GetWorkoutsByCategoryQueryValidator : AbstractValidator<GetWorkoutsByCategoryQuery>
{
    public GetWorkoutsByCategoryQueryValidator()
    {
        RuleFor(x => x.CategoryName)
            .NotEmpty().WithMessage("CategoryName is required.");
    }
}
