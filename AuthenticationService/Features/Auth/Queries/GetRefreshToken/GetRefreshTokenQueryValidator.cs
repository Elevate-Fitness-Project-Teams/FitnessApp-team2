using FluentValidation;

namespace AuthenticationService.Features.Auth.Queries.GetRefreshToken;

public class GetRefreshTokenQueryValidator : AbstractValidator<GetRefreshTokenQuery>
{
    public GetRefreshTokenQueryValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("Token is required.");
    }
}
