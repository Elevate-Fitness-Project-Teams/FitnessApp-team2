namespace SubscriptionBillingService.Contracts;

public record SubscriptionUpgradedEvent(
    Guid UserId,
    string PlanTier,
    int DurationMonths,
    DateTime UpgradedAtUtc,
    DateTime ExpiresAtUtc,
    string TransactionReference);

public record SubscriptionCancelledEvent(
    Guid UserId,
    DateTime CancelledAtUtc,
    DateTime EffectiveExpiresAtUtc);

public record SubscriptionExpiredEvent(
    Guid UserId,
    DateTime ExpiredAtUtc);

public record ExecutePaymobRefundCommandEvent(
    Guid RefundRequestId,
    Guid UserId,
    Guid PaymentAttemptId,
    string ProviderTransactionId,
    int AmountCents,
    string Currency,
    string Reason);

public record PaymentRefundedEvent(
    Guid RefundRequestId,
    Guid UserId,
    Guid PaymentAttemptId,
    string ProviderTransactionId,
    string? ProviderRefundTransactionId,
    int AmountCents,
    string Currency,
    string Reason,
    DateTime RefundedAtUtc);
