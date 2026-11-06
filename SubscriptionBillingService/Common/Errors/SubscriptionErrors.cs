namespace SubscriptionBillingService.Common.Errors;

public static class SubscriptionErrors
{
    public static Error NotFound(string resource = "Subscription") =>
        Error.NotFound("RES_NOT_FOUND", $"{resource} was not found.");

    public static Error NoActiveSubscription() =>
        Error.NotFound("RES_NOT_FOUND", "No active subscription found to cancel or refund.");

    public static Error AlreadyActive() =>
        Error.Conflict("SUB_ALREADY_ACTIVE", "User already has an active premium subscription.");

    public static Error DatabaseError(string details = "Database operation failed.") =>
        Error.Failure("SRV_DATABASE_ERROR", details);

    public static Error ProviderError(string details = "Payment gateway provider returned an error.") =>
        Error.Unavailable("PAY_PROVIDER_ERROR", details);

    public static Error InvalidSignature() =>
        Error.Unauthorized("AUTH_SIGNATURE_INVALID", "HMAC signature verification failed for webhook callback.");

    public static Error InvalidPlan(string message = "The selected plan tier or duration is invalid.") =>
        Error.Validation("PAY_INVALID_PLAN", message);

    public static Error PaymentFailed(string reason) =>
        Error.Failure("SRV_DATABASE_ERROR", $"Payment authorization failed: {reason}");

    public static Error DuplicateTransaction(string txId) =>
        Error.Conflict("PAY_TRANSACTION_ALREADY_PROCESSED", $"Transaction {txId} has already been processed.");

    public static Error RefundAlreadyProcessed(string txId) =>
        Error.Conflict("REFUND_ALREADY_PROCESSED", $"Refund for transaction {txId} is already in progress or completed.");

    public static Error RefundNotEligible(string reason) =>
        Error.Validation("REFUND_NOT_ELIGIBLE", reason);

    public static Error UnauthorizedAccess() =>
        Error.Unauthorized("AUTH_UNAUTHORIZED", "You are not authorized to perform this operation.");
}
