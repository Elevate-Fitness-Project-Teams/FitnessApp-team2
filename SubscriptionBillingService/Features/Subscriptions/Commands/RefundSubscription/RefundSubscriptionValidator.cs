using FluentValidation;

namespace SubscriptionBillingService.Features.Subscriptions.Commands.RefundSubscription;

public class RefundSubscriptionValidator : AbstractValidator<RefundSubscriptionCommand>
{
    public RefundSubscriptionValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode("VAL_USER_ID_REQUIRED")
            .WithMessage("UserId is required.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500)
            .WithErrorCode("VAL_REFUND_REASON_REQUIRED")
            .WithMessage("Refund reason is required.");
    }
}
