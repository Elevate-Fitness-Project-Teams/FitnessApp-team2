using FluentValidation;

namespace WorkoutService.Features.WorkoutSessions.Commands.StartSession;

public class StartSessionCommandValidator : AbstractValidator<StartSessionCommand>
{
    public StartSessionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be greater than 0.");

        RuleFor(x => x.WorkoutId)
            .GreaterThan(0).WithMessage("WorkoutId must be greater than 0.");
    }
}
