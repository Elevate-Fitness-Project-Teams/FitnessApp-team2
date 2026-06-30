using FluentValidation;

namespace AuthenticationService.Features.Auth.Orchestrators.ForgotPassword;

public class ForgotPasswordOrchestratorValidator : AbstractValidator<ForgotPasswordOrchestrator>
{
    public ForgotPasswordOrchestratorValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("Email is required.")
            .EmailAddress().WithErrorCode("VAL_INVALID_EMAIL").WithMessage("Email format is invalid.");
    }
}