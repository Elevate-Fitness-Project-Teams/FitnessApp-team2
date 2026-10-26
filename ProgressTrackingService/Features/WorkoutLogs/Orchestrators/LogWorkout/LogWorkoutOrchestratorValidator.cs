using FluentValidation;
using ProgressTrackingService.Common.Errors;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.WorkoutLogs.Orchestrators.LogWorkout;

public class LogWorkoutOrchestratorValidator : AbstractValidator<LogWorkoutOrchestrator>
{
    public LogWorkoutOrchestratorValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode(ProgressErrors.ValRequiredField)
            .WithMessage("UserId is required.");

        RuleFor(x => x.WorkoutId)
            .NotEmpty()
            .WithErrorCode(ProgressErrors.ValRequiredField)
            .WithMessage("WorkoutId is required.");

        RuleFor(x => x.SessionId)
            .NotEmpty()
            .WithErrorCode(ProgressErrors.ValRequiredField)
            .WithMessage("SessionId is required.");

        RuleFor(x => x.Duration)
            .GreaterThan(0)
            .WithErrorCode(ProgressErrors.ValRequiredField)
            .WithMessage("Duration must be greater than zero.");

        RuleFor(x => x.CaloriesBurned)
            .GreaterThan(0)
            .WithErrorCode(ProgressErrors.ValRequiredField)
            .WithMessage("Calories burned must be greater than zero.");

        RuleFor(x => x.Difficulty)
            .NotEmpty()
            .WithErrorCode(ProgressErrors.ValRequiredField)
            .Must(d => Enum.TryParse<WorkoutDifficulty>(d, true, out _))
            .WithErrorCode(ProgressErrors.ValInvalidDifficulty)
            .WithMessage("Invalid workout difficulty.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithErrorCode(ProgressErrors.ValInvalidRating)
            .WithMessage("Rating must be between 1 and 5.");

        RuleFor(x => x.CompletedAt)
            .NotEmpty()
            .WithErrorCode(ProgressErrors.ValRequiredField)
            .Must(date => date <= DateTime.UtcNow.AddMinutes(5))
            .WithErrorCode(ProgressErrors.ValInvalidDate)
            .WithMessage("CompletedAt cannot be in the future.");

        RuleForEach(x => x.ExercisesCompleted).ChildRules(ex =>
        {
            ex.RuleFor(e => e.ExerciseId)
                .NotEmpty()
                .WithErrorCode(ProgressErrors.ValRequiredField)
                .WithMessage("ExerciseId is required.");

            ex.RuleFor(e => e.Sets)
                .GreaterThanOrEqualTo(0)
                .WithErrorCode(ProgressErrors.ValRequiredField)
                .WithMessage("Sets must be non-negative.");

            ex.RuleFor(e => e.Reps)
                .GreaterThanOrEqualTo(0)
                .WithErrorCode(ProgressErrors.ValRequiredField)
                .WithMessage("Reps must be non-negative.");
        });
    }
}
