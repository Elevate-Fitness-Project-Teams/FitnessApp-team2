using FluentValidation;

namespace WorkoutService.Features.WorkoutSessions.Commands.CompleteSession;

public class CompleteSessionCommandValidator : AbstractValidator<CompleteSessionCommand>
{
    public CompleteSessionCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("SessionId is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId must be greater than 0.");
    }
}

