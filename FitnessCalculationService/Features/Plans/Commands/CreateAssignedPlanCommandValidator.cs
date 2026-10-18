using FitnessCalculationService.Common.Errors;
using FluentValidation;

namespace FitnessCalculationService.Features.Plans.Commands;

public class CreateAssignedPlanCommandValidator : AbstractValidator<CreateAssignedPlanCommand>
{
    public CreateAssignedPlanCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.ValRequiredField)
            .WithMessage("UserId is required.");

        RuleFor(x => x.FitnessPlanConfigId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.ValRequiredField)
            .WithMessage("FitnessPlanConfigId is required.");
    }
}
