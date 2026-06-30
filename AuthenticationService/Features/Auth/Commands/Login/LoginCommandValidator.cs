using FluentValidation;

namespace AuthenticationService.Features.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("Email is required.")
            .EmailAddress().WithErrorCode("VAL_INVALID_EMAIL").WithMessage("Email format is invalid.");

        RuleFor(x => x.Password)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("Password is required.");
    }
}