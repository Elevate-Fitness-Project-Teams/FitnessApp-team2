using FluentValidation;

namespace AuthenticationService.Features.Auth.Commands.CreateOtp;

public class CreateOtpCommandValidator : AbstractValidator<CreateOtpCommand>
{
    public CreateOtpCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("Email is required.")
            .EmailAddress().WithErrorCode("VAL_INVALID_EMAIL").WithMessage("Email format is invalid.");
    }
}
