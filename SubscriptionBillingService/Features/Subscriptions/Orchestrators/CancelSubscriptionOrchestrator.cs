using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using SubscriptionBillingService.Common;
using SubscriptionBillingService.Common.Database;
using SubscriptionBillingService.Common.Errors;
using SubscriptionBillingService.Contracts;
using SubscriptionBillingService.Domain.Entities;
using SubscriptionBillingService.Domain.Enums;
using SubscriptionBillingService.Features.Subscriptions.Commands.CancelSubscription;

namespace SubscriptionBillingService.Features.Subscriptions.Orchestrators;

public class CancelSubscriptionOrchestrator : IRequestHandler<CancelSubscriptionCommand, Result<CancelSubscriptionResponse>>
{
    private readonly IGenericRepository<UserSubscription> _subscriptionRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CancelSubscriptionOrchestrator> _logger;

    public CancelSubscriptionOrchestrator(
        IGenericRepository<UserSubscription> subscriptionRepo,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint,
        ILogger<CancelSubscriptionOrchestrator> logger)
    {
        _subscriptionRepo = subscriptionRepo;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<Result<CancelSubscriptionResponse>> Handle(
        CancelSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepo.FirstOrDefaultAsync(
            s => s.UserId == request.UserId,
            cancellationToken);

        // Acceptance Criteria: No active subscription to cancel -> Return 404 - RES_NOT_FOUND
        if (subscription == null ||
            subscription.Tier != SubscriptionTier.Premium ||
            subscription.Status != SubscriptionStatus.Active ||
            !subscription.ExpiresAtUtc.HasValue ||
            subscription.ExpiresAtUtc.Value <= DateTime.UtcNow)
        {
            _logger.LogWarning("No active Premium subscription found to cancel for user {UserId}", request.UserId);
            return Result<CancelSubscriptionResponse>.Failure(SubscriptionErrors.NoActiveSubscription());
        }

        var cancelledAt = DateTime.UtcNow;

        await _unitOfWork.ExecuteAsync(async () =>
        {
            // Turn off auto-renewal without truncating current paid period
            subscription.AutoRenew = false;
            subscription.Status = SubscriptionStatus.Cancelled;
            subscription.CancelledAtUtc = cancelledAt;

            _subscriptionRepo.Update(subscription);

            // Stage Outbox event
            await _publishEndpoint.Publish(new SubscriptionCancelledEvent(
                subscription.UserId,
                cancelledAt,
                subscription.ExpiresAtUtc.Value), cancellationToken);
        }, cancellationToken);

        _logger.LogInformation("Cancelled auto-renewal for user {UserId}. Premium remains active until {ExpiresAt}",
            subscription.UserId, subscription.ExpiresAtUtc);

        return Result<CancelSubscriptionResponse>.Success(new CancelSubscriptionResponse(
            subscription.UserId,
            subscription.Tier.ToString(),
            subscription.Status.ToString(),
            subscription.AutoRenew,
            subscription.ExpiresAtUtc,
            cancelledAt));
    }
}
