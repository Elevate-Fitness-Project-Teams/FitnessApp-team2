using MediatR;
using SubscriptionBillingService.Common;
using SubscriptionBillingService.Domain.Enums;

namespace SubscriptionBillingService.Features.Subscriptions.Commands.UpgradeSubscription;

public record UpgradeSubscriptionCommand(
    Guid UserId,
    SubscriptionTier PlanTier,
    int DurationMonths,
    string? IdempotencyKey = null) : IRequest<Result<UpgradeSubscriptionResponse>>;

public record UpgradeSubscriptionResponse(
    Guid PaymentAttemptId,
    string ProviderIntentionId,
    string CheckoutUrl,
    string ClientSecret,
    decimal Amount,
    string Currency,
    string Status,
    DateTime ExpiresAt);
