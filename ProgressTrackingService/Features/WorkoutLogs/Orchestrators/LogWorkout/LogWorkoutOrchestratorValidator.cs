using FluentValidation;

namespace ProgressTrackingService.Features.WorkoutLogs.Orchestrators.LogWorkout;

public class LogWorkoutOrchestratorValidator : AbstractValidator<LogWorkoutOrchestrator>
{
    public LogWorkoutOrchestratorValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
        RuleFor(x => x.WorkoutId).NotEmpty().WithMessage("WorkoutId is required.");
        RuleFor(x => x.SessionId).NotEmpty().WithMessage("SessionId is required.");
        RuleFor(x => x.Duration).GreaterThan(0).WithMessage("Duration must be greater than zero.");
        RuleFor(x => x.CaloriesBurned).GreaterThan(0).WithMessage("Calories burned must be greater than zero.");
        RuleFor(x => x.Rating).InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");
        RuleFor(x => x.CompletedAt)
            .Must(date => date <= DateTime.UtcNow)
            .WithMessage("CompletedAt cannot be in the future.");
    }
}
