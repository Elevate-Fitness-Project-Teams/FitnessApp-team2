using FluentValidation;

namespace SubscriptionBillingService.Features.Subscriptions.Queries.GetSubscriptionStatus;

public class GetSubscriptionStatusValidator : AbstractValidator<GetSubscriptionStatusQuery>
{
    public GetSubscriptionStatusValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode("VAL_USER_ID_REQUIRED")
            .WithMessage("UserId is required.");
    }
}
