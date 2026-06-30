using FluentValidation;

namespace WorkoutService.Features.Exercises.Queries.GetExerciseById;

public class GetExerciseByIdQueryValidator : AbstractValidator<GetExerciseByIdQuery>
{
    public GetExerciseByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Exercise Id must be greater than 0.");
    }
}
