using FitnessCalculationService.Common.Errors;
using FluentValidation;

namespace FitnessCalculationService.Features.Calculations.Commands.RecalculateMetrics;

public class RecalculateMetricsOrchestratorValidator : AbstractValidator<RecalculateMetricsOrchestrator>
{
    public RecalculateMetricsOrchestratorValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.ValRequiredField)
            .WithMessage("UserId is required.");

        When(x => x.NewWeight.HasValue, () =>
        {
            RuleFor(x => x.NewWeight!.Value)
                .InclusiveBetween(30.0, 300.0)
                .WithErrorCode(ErrorCodes.ValInvalidWeight)
                .WithMessage("Weight must be between 30 and 300 kg.");
        });

        RuleFor(x => x.TriggeredBy)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.ValRequiredField)
            .WithMessage("TriggeredBy is required.");

        When(x => x.SourceEventId.HasValue, () =>
        {
            RuleFor(x => x.SourceEventId!.Value)
                .NotEmpty()
                .WithErrorCode(ErrorCodes.ValRequiredField)
                .WithMessage("SourceEventId cannot be empty when provided.");
        });
    }
}
