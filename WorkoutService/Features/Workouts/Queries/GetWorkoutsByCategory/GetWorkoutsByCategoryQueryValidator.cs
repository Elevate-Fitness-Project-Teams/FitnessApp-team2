using FluentValidation;

namespace WorkoutService.Features.Workouts.Queries.GetWorkoutsByCategory;

public class GetWorkoutsByCategoryQueryValidator : AbstractValidator<GetWorkoutsByCategoryQuery>
{
	public GetWorkoutsByCategoryQueryValidator()
	{
		RuleFor(x => x.CategoryName)
			.NotEmpty().WithMessage("CategoryName is required.");
		RuleFor(x => x.Page)
			.GreaterThan(0).WithMessage("Page number must be greater than 0.");

		RuleFor(x => x.PageSize)
			.GreaterThan(0).WithMessage("Page size must be greater than 0.")
			.LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100.");
	}
}
