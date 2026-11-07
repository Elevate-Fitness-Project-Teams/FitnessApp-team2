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
