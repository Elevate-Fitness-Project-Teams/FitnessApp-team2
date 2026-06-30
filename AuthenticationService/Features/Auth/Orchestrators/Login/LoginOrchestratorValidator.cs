using FluentValidation;

namespace AuthenticationService.Features.Auth.Orchestrators.Login;

public class LoginOrchestratorValidator : AbstractValidator<LoginOrchestrator>
{
    public LoginOrchestratorValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("Email is required.")
            .EmailAddress().WithErrorCode("VAL_INVALID_EMAIL").WithMessage("Email format is invalid.");

        RuleFor(x => x.Password)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("Password is required.");
    }
}