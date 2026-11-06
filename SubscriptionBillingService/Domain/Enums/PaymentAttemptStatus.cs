namespace SubscriptionBillingService.Domain.Enums;

public enum PaymentAttemptStatus
{
    Initiated,
    PendingRedirect,
    Succeeded,
    Failed,
    Superseded,
    DuplicateChargePendingRefund,
    AutoRefunded,
    Refunded,
    Expired
}
