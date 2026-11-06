using SubscriptionBillingService.Domain.Enums;

namespace SubscriptionBillingService.Domain.Catalog;

public record SubscriptionPlanDetails(
    SubscriptionTier Tier,
    int DurationMonths,
    int AmountCents,
    string Currency,
    string Name,
    string Description)
{
    public decimal Amount => AmountCents / 100m;
}

public static class SubscriptionPlanCatalog
{
    private static readonly Dictionary<(SubscriptionTier, int), SubscriptionPlanDetails> Plans = new()
    {
        [(SubscriptionTier.Premium, 1)] = new(
            SubscriptionTier.Premium, 1, 19900, "EGP",
            "Elevate Fitness Premium (1 Month)",
            "Unlimited workout tracking, AI fitness intelligence, and 2000 req/hr rate limit."),
        
        [(SubscriptionTier.Premium, 3)] = new(
            SubscriptionTier.Premium, 3, 49900, "EGP",
            "Elevate Fitness Premium (3 Months)",
            "Quarterly premium subscription with priority AI coaching."),
        
        [(SubscriptionTier.Premium, 6)] = new(
            SubscriptionTier.Premium, 6, 89900, "EGP",
            "Elevate Fitness Premium (6 Months)",
            "Semi-annual premium access with maximum performance insights."),
        
        [(SubscriptionTier.Premium, 12)] = new(
            SubscriptionTier.Premium, 12, 149900, "EGP",
            "Elevate Fitness Premium (Annual)",
            "Full 1-year premium membership with highest savings and perks.")
    };

    public static bool TryGetPlan(SubscriptionTier tier, int durationMonths, out SubscriptionPlanDetails? plan)
    {
        return Plans.TryGetValue((tier, durationMonths), out plan);
    }

    public static SubscriptionPlanDetails GetPlanOrThrow(SubscriptionTier tier, int durationMonths)
    {
        if (TryGetPlan(tier, durationMonths, out var plan))
        {
            return plan!;
        }

        throw new ArgumentException($"Invalid subscription plan: Tier={tier}, DurationMonths={durationMonths}");
    }

    public static IReadOnlyCollection<SubscriptionPlanDetails> GetAllPlans() => Plans.Values;
}
