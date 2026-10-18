using FitnessCalculationService.Common.Errors;
using FluentValidation;

namespace FitnessCalculationService.Features.Plans.Orchestrators;

public class AssignPlanOrchestratorValidator : AbstractValidator<AssignPlanOrchestrator>
{
    public AssignPlanOrchestratorValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.ValRequiredField)
            .WithMessage("UserId is required.");

        RuleFor(x => x.TriggeredBy)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.ValRequiredField)
            .WithMessage("TriggeredBy is required.");
    }
}
