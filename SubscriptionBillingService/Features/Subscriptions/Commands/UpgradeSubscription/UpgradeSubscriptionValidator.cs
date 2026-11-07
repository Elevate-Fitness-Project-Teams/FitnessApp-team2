using FluentValidation;
using SubscriptionBillingService.Domain.Catalog;
using SubscriptionBillingService.Domain.Enums;

namespace SubscriptionBillingService.Features.Subscriptions.Commands.UpgradeSubscription;

public class UpgradeSubscriptionValidator : AbstractValidator<UpgradeSubscriptionCommand>
{
    public UpgradeSubscriptionValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode("VAL_USER_ID_REQUIRED")
            .WithMessage("UserId is required.");

        RuleFor(x => x.PlanTier)
            .Equal(SubscriptionTier.Premium)
            .WithErrorCode("PAY_INVALID_PLAN")
            .WithMessage("Only Premium tier upgrade is supported.");

        RuleFor(x => x.DurationMonths)
            .Must((cmd, duration) => SubscriptionPlanCatalog.TryGetPlan(cmd.PlanTier, duration, out _))
            .WithErrorCode("PAY_INVALID_PLAN")
            .WithMessage("Invalid plan duration. Supported durations are 1, 3, 6, or 12 months.");
    }
}
