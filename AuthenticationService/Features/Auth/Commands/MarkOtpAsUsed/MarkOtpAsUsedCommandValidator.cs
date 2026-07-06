using FluentValidation;

namespace AuthenticationService.Features.Auth.Commands.MarkOtpAsUsed;

public class MarkOtpAsUsedCommandValidator : AbstractValidator<MarkOtpAsUsedCommand>
{
    public MarkOtpAsUsedCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("Email is required.")
            .EmailAddress().WithErrorCode("VAL_INVALID_EMAIL").WithMessage("Email format is invalid.");
            
        RuleFor(x => x.Otp)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("OTP is required.");
    }
}
