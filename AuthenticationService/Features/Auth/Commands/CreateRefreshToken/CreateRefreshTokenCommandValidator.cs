using FluentValidation;

namespace AuthenticationService.Features.Auth.Commands.CreateRefreshToken;

public class CreateRefreshTokenCommandValidator : AbstractValidator<CreateRefreshTokenCommand>
{
    public CreateRefreshTokenCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("User ID is required.");
            
        RuleFor(x => x.Token)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("Token is required.");
            
        RuleFor(x => x.ExpiresAt)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("Expiration date is required.");
    }
}
