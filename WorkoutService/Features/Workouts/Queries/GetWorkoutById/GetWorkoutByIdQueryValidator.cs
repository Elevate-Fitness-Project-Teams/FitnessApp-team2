using FluentValidation;

namespace WorkoutService.Features.Workouts.Queries.GetWorkoutById;

public class GetWorkoutByIdQueryValidator : AbstractValidator<GetWorkoutByIdQuery>
{
    public GetWorkoutByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Workout Id must be greater than 0.");
    }
}

