using FluentValidation;

namespace AuthenticationService.Features.Auth.Orchestrators.Logout;

public class LogoutOrchestratorValidator : AbstractValidator<LogoutOrchestrator>
{
    public LogoutOrchestratorValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}