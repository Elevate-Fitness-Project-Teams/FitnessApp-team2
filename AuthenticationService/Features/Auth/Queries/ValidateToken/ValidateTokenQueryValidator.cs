using FluentValidation;

namespace AuthenticationService.Features.Auth.Queries.ValidateToken;

public class ValidateTokenQueryValidator : AbstractValidator<ValidateTokenQuery>
{
    public ValidateTokenQueryValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("Token is required.");
    }
}
