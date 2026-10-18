using FitnessCalculationService.Common.Errors;
using FluentValidation;

namespace FitnessCalculationService.Features.Plans.Queries;

public class GetActiveAssignedPlanQueryValidator : AbstractValidator<GetActiveAssignedPlanQuery>
{
    public GetActiveAssignedPlanQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.ValRequiredField)
            .WithMessage("UserId is required.");
    }
}
