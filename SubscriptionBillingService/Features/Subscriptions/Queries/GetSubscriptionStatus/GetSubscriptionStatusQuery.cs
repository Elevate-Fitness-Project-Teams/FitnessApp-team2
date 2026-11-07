using MediatR;
using SubscriptionBillingService.Common;

namespace SubscriptionBillingService.Features.Subscriptions.Queries.GetSubscriptionStatus;

public record GetSubscriptionStatusQuery(Guid UserId) : IRequest<Result<GetSubscriptionStatusResponse>>;

public record GetSubscriptionStatusResponse(
    Guid UserId,
    string Tier,
    string Status,
    bool AutoRenew,
    DateTime? ExpiresAt);
