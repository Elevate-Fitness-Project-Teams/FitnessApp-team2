using FluentValidation;

namespace AuthenticationService.Features.Auth.Queries.GetRecentFailedLoginAttempts;

public class GetRecentFailedLoginAttemptsQueryValidator : AbstractValidator<GetRecentFailedLoginAttemptsQuery>
{
    public GetRecentFailedLoginAttemptsQueryValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("Email is required.")
            .EmailAddress().WithErrorCode("VAL_INVALID_EMAIL").WithMessage("Email format is invalid.");
            
        RuleFor(x => x.CutoffTime)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("Cutoff time is required.");
    }
}
