using MediatR;
using SubscriptionBillingService.Common;
using SubscriptionBillingService.Common.Database;
using SubscriptionBillingService.Domain.Entities;
using SubscriptionBillingService.Domain.Enums;

namespace SubscriptionBillingService.Features.Subscriptions.Queries.GetSubscriptionStatus;

public class GetSubscriptionStatusQueryHandler : IRequestHandler<GetSubscriptionStatusQuery, Result<GetSubscriptionStatusResponse>>
{
    private readonly IGenericRepository<UserSubscription> _subscriptionRepo;

    public GetSubscriptionStatusQueryHandler(IGenericRepository<UserSubscription> subscriptionRepo)
    {
        _subscriptionRepo = subscriptionRepo;
    }

    public async Task<Result<GetSubscriptionStatusResponse>> Handle(
        GetSubscriptionStatusQuery request,
        CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepo.FirstOrDefaultAsync(
            s => s.UserId == request.UserId,
            cancellationToken);

        if (subscription == null)
        {
            // Acceptance Criteria: No subscription row exists (defaults to Free) -> Return 200 with Tier: 'Free'
            return Result<GetSubscriptionStatusResponse>.Success(new GetSubscriptionStatusResponse(
                request.UserId,
                SubscriptionTier.Free.ToString(),
                "None",
                false,
                null));
        }

        // Read-time expiration check
        if (subscription.ExpiresAtUtc.HasValue && subscription.ExpiresAtUtc.Value <= DateTime.UtcNow)
        {
            return Result<GetSubscriptionStatusResponse>.Success(new GetSubscriptionStatusResponse(
                subscription.UserId,
                SubscriptionTier.Free.ToString(),
                SubscriptionStatus.Expired.ToString(),
                false,
                subscription.ExpiresAtUtc));
        }

        return Result<GetSubscriptionStatusResponse>.Success(new GetSubscriptionStatusResponse(
            subscription.UserId,
            subscription.Tier.ToString(),
            subscription.Status.ToString(),
            subscription.AutoRenew,
            subscription.ExpiresAtUtc));
    }
}
