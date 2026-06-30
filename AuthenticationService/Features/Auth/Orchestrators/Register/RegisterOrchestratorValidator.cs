using FluentValidation;

namespace AuthenticationService.Features.Auth.Orchestrators.Register;

public class RegisterOrchestratorValidator : AbstractValidator<RegisterOrchestrator>
{
    public RegisterOrchestratorValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("Email is required.")
            .EmailAddress().WithErrorCode("VAL_INVALID_EMAIL").WithMessage("Email format is invalid.");

        RuleFor(x => x.Password)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("Password is required.")
            .MinimumLength(8).WithErrorCode("VAL_MIN_LENGTH")
            .WithMessage("Password must be at least 8 characters long.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("First name is required.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("Last name is required.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("Phone number is required.");
    }
}