using FitnessCalculationService.Common.Errors;
using FluentValidation;

namespace FitnessCalculationService.Features.Plans.Queries.GetActivePlan;

public class GetActivePlanQueryValidator : AbstractValidator<GetActivePlanQuery>
{
    public GetActivePlanQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.ValRequiredField)
            .WithMessage("UserId is required.");
    }
}
