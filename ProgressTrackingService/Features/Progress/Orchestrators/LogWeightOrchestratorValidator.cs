using FluentValidation;

using ProgressTrackingService.Features.Progress.Orchestrators;

namespace ProgressTrackingService.Features.Progress.Orchestrators;

public class LogWeightOrchestratorValidator : AbstractValidator<LogWeightOrchestrator>
{
    public LogWeightOrchestratorValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(x => x.Weight)
            .InclusiveBetween(40, 200)
            .WithMessage("Weight must be between 40 and 200 kg.")
            .WithErrorCode("VAL_INVALID_WEIGHT");

        RuleFor(x => x.Date)
            .Must(date => date <= DateTime.UtcNow)
            .WithMessage("Date cannot be in the future.");
    }
}
