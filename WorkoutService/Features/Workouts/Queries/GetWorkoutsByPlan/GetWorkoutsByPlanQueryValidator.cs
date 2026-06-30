using FluentValidation;

namespace WorkoutService.Features.Workouts.Queries.GetWorkoutsByPlan;

public class GetWorkoutsByPlanQueryValidator : AbstractValidator<GetWorkoutsByPlanQuery>
{
    public GetWorkoutsByPlanQueryValidator()
    {
        RuleFor(x => x.PlanId)
            .GreaterThan(0).WithMessage("Plan Id must be greater than 0.");
    }
}
