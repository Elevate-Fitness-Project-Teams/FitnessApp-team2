using FluentValidation;

namespace AuthenticationService.Features.Auth.Commands.LogLoginAttempt;

public class LogLoginAttemptCommandValidator : AbstractValidator<LogLoginAttemptCommand>
{
    public LogLoginAttemptCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("Email is required.")
            .EmailAddress().WithErrorCode("VAL_INVALID_EMAIL").WithMessage("Email format is invalid.");
            
        RuleFor(x => x.IpAddress)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("IP Address is required.");
    }
}
