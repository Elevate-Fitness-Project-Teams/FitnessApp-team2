using FluentValidation;
using ProgressTrackingService.Common.Errors;

namespace ProgressTrackingService.Features.Progress.Orchestrators;

public class LogWeightOrchestratorValidator : AbstractValidator<LogWeightOrchestrator>
{
    public LogWeightOrchestratorValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode(ProgressErrors.ValRequiredField)
            .WithMessage("UserId is required.");

        RuleFor(x => x.Weight)
            .InclusiveBetween(40, 200)
            .WithErrorCode(ProgressErrors.ValInvalidWeight)
            .WithMessage("Weight must be between 40 and 200 kg.");

        RuleFor(x => x.Date)
            .NotEmpty()
            .WithErrorCode(ProgressErrors.ValRequiredField)
            .Must(date => date >= new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc) && date <= DateTime.UtcNow.AddMinutes(5))
            .WithErrorCode(ProgressErrors.ValInvalidDate)
            .WithMessage("Date must be between year 2000 and present.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithErrorCode(ProgressErrors.ValRequiredField)
            .WithMessage("Notes cannot exceed 500 characters.");
    }
}
