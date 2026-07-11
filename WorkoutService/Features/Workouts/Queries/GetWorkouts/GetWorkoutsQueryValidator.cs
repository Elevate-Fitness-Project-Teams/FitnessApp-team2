using FluentValidation;

namespace WorkoutService.Features.Workouts.Queries.GetWorkouts;

public class GetWorkoutsQueryValidator : AbstractValidator<GetWorkoutsQuery>
{
    public GetWorkoutsQueryValidator()
    {
        RuleFor(x => x.Page)
            .NotEmpty().WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .NotEmpty().WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100.");
            
        RuleFor(x => x.Duration)
            .NotEmpty().When(x => x.Duration.HasValue).WithMessage("Duration must be greater than 0.");
    }
}

