using FluentValidation;

namespace AuthenticationService.Features.Auth.Commands.UpdateUserLockout;

public class UpdateUserLockoutCommandValidator : AbstractValidator<UpdateUserLockoutCommand>
{
    public UpdateUserLockoutCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("Email is required.")
            .EmailAddress().WithErrorCode("VAL_INVALID_EMAIL").WithMessage("Email format is invalid.");
    }
}
