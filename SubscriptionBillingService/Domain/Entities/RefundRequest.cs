using System.ComponentModel.DataAnnotations;
using SubscriptionBillingService.Domain.Enums;

namespace SubscriptionBillingService.Domain.Entities;

public class RefundRequest : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid PaymentAttemptId { get; set; }
    public string ProviderTransactionId { get; set; } = string.Empty;
    public string? ProviderRefundTransactionId { get; set; }
    public int AmountCents { get; set; }
    public string Currency { get; set; } = "EGP";
    public string Reason { get; set; } = string.Empty;
    public RefundType RefundType { get; set; } = RefundType.AutoDuplicateRefund;
    public RefundRequestStatus Status { get; set; } = RefundRequestStatus.Pending;
    public int RetryCount { get; set; } = 0;
    public string? LastError { get; set; }
    public DateTime? CompletedAtUtc { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public PaymentAttempt? PaymentAttempt { get; set; }
}
