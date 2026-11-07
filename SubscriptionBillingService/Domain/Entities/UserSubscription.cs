using System.ComponentModel.DataAnnotations;
using SubscriptionBillingService.Domain.Enums;

namespace SubscriptionBillingService.Domain.Entities;

public class UserSubscription : BaseEntity
{
    public Guid UserId { get; set; }
    public SubscriptionTier Tier { get; set; } = SubscriptionTier.Free;
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    public int PlanDurationMonths { get; set; } = 0;
    public bool AutoRenew { get; set; } = true;
    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAtUtc { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public string? LastProviderTransactionId { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public ICollection<PaymentAttempt> PaymentAttempts { get; set; } = new List<PaymentAttempt>();
}
