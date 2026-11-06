namespace SubscriptionBillingService.Domain.Enums;

public enum BillingPaymentStatus
{
    Success,
    Failed,
    DuplicateChargeDetected,
    Refunded
}
