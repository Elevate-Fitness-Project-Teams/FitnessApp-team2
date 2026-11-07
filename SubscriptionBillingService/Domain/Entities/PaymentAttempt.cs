using System.ComponentModel.DataAnnotations;
using SubscriptionBillingService.Domain.Enums;

namespace SubscriptionBillingService.Domain.Entities;

public class PaymentAttempt : BaseEntity
{
    public Guid UserId { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public SubscriptionTier PlanTier { get; set; } = SubscriptionTier.Premium;
    public int DurationMonths { get; set; }
    public int AmountCents { get; set; }
    public string Currency { get; set; } = "EGP";
    public PaymentAttemptStatus Status { get; set; } = PaymentAttemptStatus.Initiated;
    public string? ProviderIntentionId { get; set; }
    public string? ProviderOrderId { get; set; }
    public string? ProviderTransactionId { get; set; }
    public string? ClientSecret { get; set; }
    public string? CheckoutUrl { get; set; }
    public string? FailureReason { get; set; }
    public DateTime ExpiresAtUtc { get; set; } = DateTime.UtcNow.AddMinutes(30);
    public DateTime? CompletedAtUtc { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public UserSubscription? UserSubscription { get; set; }
    public ICollection<BillingLog> BillingLogs { get; set; } = new List<BillingLog>();
    public RefundRequest? RefundRequest { get; set; }
}
