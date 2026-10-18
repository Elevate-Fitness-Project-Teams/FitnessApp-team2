using FitnessCalculationService.Common.Errors;
using FluentValidation;

namespace FitnessCalculationService.Features.Plans.Queries;

public class GetPlanConfigByGoalStatusQueryValidator : AbstractValidator<GetPlanConfigByGoalStatusQuery>
{
    public GetPlanConfigByGoalStatusQueryValidator()
    {
        RuleFor(x => x.Goal)
            .IsInEnum()
            .WithErrorCode(ErrorCodes.ValInvalidGoal)
            .WithMessage("Invalid fitness goal.");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithErrorCode(ErrorCodes.ValRequiredField)
            .WithMessage("Invalid calorie status.");
    }
}
