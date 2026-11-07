using SubscriptionBillingService.Domain.Enums;

namespace SubscriptionBillingService.Domain.Entities;

public class BillingLog : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid PaymentAttemptId { get; set; }
    public SubscriptionTier PlanTier { get; set; } = SubscriptionTier.Premium;
    public int DurationMonths { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EGP";
    public string PaymentProvider { get; set; } = "Paymob";
    public string ProviderTransactionId { get; set; } = string.Empty;
    public string? ProviderIntentionId { get; set; }
    public BillingPaymentStatus PaymentStatus { get; set; } = BillingPaymentStatus.Success;
    public string? RawPayloadHash { get; set; }
    public string? FailureReason { get; set; }

    // Navigation properties
    public PaymentAttempt? PaymentAttempt { get; set; }
}
