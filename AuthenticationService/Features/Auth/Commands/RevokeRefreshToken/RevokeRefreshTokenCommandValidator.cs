using FluentValidation;

namespace AuthenticationService.Features.Auth.Commands.RevokeRefreshToken;

public class RevokeRefreshTokenCommandValidator : AbstractValidator<RevokeRefreshTokenCommand>
{
    public RevokeRefreshTokenCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("Token is required.");
    }
}
