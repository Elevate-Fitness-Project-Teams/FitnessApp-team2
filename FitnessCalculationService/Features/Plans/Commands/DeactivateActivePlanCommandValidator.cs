using FitnessCalculationService.Common.Errors;
using FluentValidation;

namespace FitnessCalculationService.Features.Plans.Commands;

public class DeactivateActivePlanCommandValidator : AbstractValidator<DeactivateActivePlanCommand>
{
    public DeactivateActivePlanCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.ValRequiredField)
            .WithMessage("UserId is required.");
    }
}
