using FluentValidation;

namespace AuthenticationService.Features.Auth.Orchestrators.SendOtp;

public class SendOtpOrchestratorValidator : AbstractValidator<SendOtpOrchestrator>
{
    public SendOtpOrchestratorValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email is required.");
    }
}