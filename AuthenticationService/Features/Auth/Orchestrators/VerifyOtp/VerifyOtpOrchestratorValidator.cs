using FluentValidation;

namespace AuthenticationService.Features.Auth.Orchestrators.VerifyOtp;

public class VerifyOtpOrchestratorValidator : AbstractValidator<VerifyOtpOrchestrator>
{
    public VerifyOtpOrchestratorValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email is required.");

        RuleFor(x => x.Otp)
            .NotEmpty().WithMessage("OTP is required.")
            .Length(6).WithMessage("OTP must be exactly 6 characters.");
    }
}