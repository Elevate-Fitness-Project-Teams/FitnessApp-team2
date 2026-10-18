using FitnessCalculationService.Common.Errors;
using FluentValidation;

namespace FitnessCalculationService.Features.Plans.Queries.GetPlanById;

public class GetPlanByIdQueryValidator : AbstractValidator<GetPlanByIdQuery>
{
    public GetPlanByIdQueryValidator()
    {
        RuleFor(x => x.PlanId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.ValRequiredField)
            .WithMessage("PlanId is required.")
            .MaximumLength(20)
            .WithErrorCode(ErrorCodes.ValRequiredField)
            .WithMessage("PlanId cannot exceed 20 characters.");
    }
}
