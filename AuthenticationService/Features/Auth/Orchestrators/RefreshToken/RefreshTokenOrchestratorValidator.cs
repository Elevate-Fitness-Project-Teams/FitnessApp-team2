using FluentValidation;

namespace AuthenticationService.Features.Auth.Orchestrators.RefreshToken;

public class RefreshTokenOrchestratorValidator : AbstractValidator<RefreshTokenOrchestrator>
{
    public RefreshTokenOrchestratorValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithErrorCode("VAL_REQUIRED_FIELD").WithMessage("Refresh Token is required.");
    }
}