using FluentValidation;

namespace WorkoutService.Features.WorkoutSessions.Orchestrators.StartSession;

public class StartSessionOrchestratorValidator : AbstractValidator<StartSessionOrchestrator>
{
    public StartSessionOrchestratorValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId must be greater than 0.");

        RuleFor(x => x.WorkoutId)
            .NotEmpty().WithMessage("WorkoutId must be greater than 0.");
    }
}

