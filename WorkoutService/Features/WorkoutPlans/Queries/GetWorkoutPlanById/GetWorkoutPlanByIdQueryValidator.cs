using FluentValidation;

namespace WorkoutService.Features.WorkoutPlans.Queries.GetWorkoutPlanById;

public class GetWorkoutPlanByIdQueryValidator : AbstractValidator<GetWorkoutPlanByIdQuery>
{
    public GetWorkoutPlanByIdQueryValidator()
    {
        RuleFor(x => x.PlanId)
            .NotEmpty().WithMessage("PlanId is required.");
    }
}

