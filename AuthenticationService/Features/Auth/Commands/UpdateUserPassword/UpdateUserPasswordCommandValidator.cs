using FluentValidation;

namespace AuthenticationService.Features.Auth.Commands.UpdateUserPassword;

public class UpdateUserPasswordCommandValidator : AbstractValidator<UpdateUserPasswordCommand>
{
    public UpdateUserPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("Email is required.")
            .EmailAddress().WithErrorCode("VAL_INVALID_EMAIL").WithMessage("Email format is invalid.");
            
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("New password is required.")
            .MinimumLength(8).WithErrorCode("VAL_MIN_LENGTH").WithMessage("New password must be at least 8 characters long.");
    }
}
