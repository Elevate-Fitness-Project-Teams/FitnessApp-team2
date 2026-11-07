using MediatR;
using SubscriptionBillingService.Common;

namespace SubscriptionBillingService.Features.Subscriptions.Commands.CancelSubscription;

public record CancelSubscriptionCommand(Guid UserId) : IRequest<Result<CancelSubscriptionResponse>>;

public record CancelSubscriptionResponse(
    Guid UserId,
    string Tier,
    string Status,
    bool AutoRenew,
    DateTime? ExpiresAt,
    DateTime CancelledAt);
