using FluentValidation;

namespace SubscriptionBillingService.Features.Subscriptions.Commands.CancelSubscription;

public class CancelSubscriptionValidator : AbstractValidator<CancelSubscriptionCommand>
{
    public CancelSubscriptionValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode("VAL_USER_ID_REQUIRED")
            .WithMessage("UserId is required.");
    }
}
