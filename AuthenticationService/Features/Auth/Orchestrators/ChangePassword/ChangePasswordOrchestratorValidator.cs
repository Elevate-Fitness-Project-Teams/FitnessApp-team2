using FluentValidation;

namespace AuthenticationService.Features.Auth.Orchestrators.ChangePassword;

public class ChangePasswordOrchestratorValidator : AbstractValidator<ChangePasswordOrchestrator>
{
    public ChangePasswordOrchestratorValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("User ID is required.");

        RuleFor(x => x.OldPassword)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("Old Password is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("New Password is required.")
            .MinimumLength(8).WithErrorCode("VAL_MIN_LENGTH")
            .WithMessage("New Password must be at least 8 characters long.");
    }
}