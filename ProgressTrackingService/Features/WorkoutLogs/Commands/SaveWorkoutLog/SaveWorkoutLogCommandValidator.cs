using FluentValidation;

namespace ProgressTrackingService.Features.WorkoutLogs.Commands.SaveWorkoutLog;

public class SaveWorkoutLogCommandValidator : AbstractValidator<SaveWorkoutLogCommand>
{
    public SaveWorkoutLogCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
        RuleFor(x => x.WorkoutId).NotEmpty().WithMessage("WorkoutId is required.");
        RuleFor(x => x.SessionId).NotEmpty().WithMessage("SessionId is required.");
        RuleFor(x => x.CompletedAt).LessThanOrEqualTo(DateTime.UtcNow).WithMessage("CompletedAt cannot be in the future.");
        RuleFor(x => x.Duration).GreaterThan(0).WithMessage("Duration must be greater than zero.");
        RuleFor(x => x.Rating).InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");
    }
}
