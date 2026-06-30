using FluentValidation;

namespace AuthenticationService.Features.Auth.Orchestrators.ResetPassword;

public class ResetPasswordOrchestratorValidator : AbstractValidator<ResetPasswordOrchestrator>
{
    public ResetPasswordOrchestratorValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("Email is required.")
            .EmailAddress().WithErrorCode("VAL_INVALID_EMAIL").WithMessage("Email format is invalid.");

        RuleFor(x => x.Otp)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("OTP is required.")
            .Length(6).WithErrorCode("VAL_INVALID_LENGTH").WithMessage("OTP must be 6 digits.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("New Password is required.")
            .MinimumLength(8).WithErrorCode("VAL_MIN_LENGTH")
            .WithMessage("New Password must be at least 8 characters long.");
    }
}