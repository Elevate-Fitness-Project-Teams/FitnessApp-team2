using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using SubscriptionBillingService.Common;
using SubscriptionBillingService.Common.Database;
using SubscriptionBillingService.Common.Errors;
using SubscriptionBillingService.Contracts;
using SubscriptionBillingService.Domain.Entities;
using SubscriptionBillingService.Domain.Enums;
using SubscriptionBillingService.Features.Subscriptions.Commands.RefundSubscription;
using SubscriptionBillingService.Infrastructure.Caching;

namespace SubscriptionBillingService.Features.Subscriptions.Orchestrators;

public class RefundSubscriptionOrchestrator : IRequestHandler<RefundSubscriptionCommand, Result<RefundSubscriptionResponse>>
{
    private readonly IGenericRepository<UserSubscription> _subscriptionRepo;
    private readonly IGenericRepository<PaymentAttempt> _attemptRepo;
    private readonly IGenericRepository<RefundRequest> _refundRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ISubscriptionTierCache _tierCache;
    private readonly ILogger<RefundSubscriptionOrchestrator> _logger;

    public RefundSubscriptionOrchestrator(
        IGenericRepository<UserSubscription> subscriptionRepo,
        IGenericRepository<PaymentAttempt> attemptRepo,
        IGenericRepository<RefundRequest> refundRepo,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint,
        ISubscriptionTierCache tierCache,
        ILogger<RefundSubscriptionOrchestrator> logger)
    {
        _subscriptionRepo = subscriptionRepo;
        _attemptRepo = attemptRepo;
        _refundRepo = refundRepo;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
        _tierCache = tierCache;
        _logger = logger;
    }

    public async Task<Result<RefundSubscriptionResponse>> Handle(
        RefundSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepo.FirstOrDefaultAsync(
            s => s.UserId == request.UserId, cancellationToken);

        if (subscription == null || string.IsNullOrWhiteSpace(subscription.LastProviderTransactionId))
        {
            _logger.LogWarning("No refundable subscription transaction found for user {UserId}", request.UserId);
            return Result<RefundSubscriptionResponse>.Failure(SubscriptionErrors.NoActiveSubscription());
        }

        var providerTxId = subscription.LastProviderTransactionId;

        // Locate successful payment attempt
        var attempt = await _attemptRepo.FirstOrDefaultAsync(
            a => a.UserId == request.UserId && a.ProviderTransactionId == providerTxId, cancellationToken);

        if (attempt == null)
        {
            _logger.LogWarning("PaymentAttempt not found for transaction {TxId}", providerTxId);
            return Result<RefundSubscriptionResponse>.Failure(SubscriptionErrors.NotFound("PaymentAttempt"));
        }

        // Check if refund was already issued
        var existingRefund = await _refundRepo.FirstOrDefaultAsync(
            r => r.ProviderTransactionId == providerTxId, cancellationToken);

        if (existingRefund != null)
        {
            _logger.LogWarning("Refund already initiated/processed for transaction {TxId}", providerTxId);
            return Result<RefundSubscriptionResponse>.Failure(SubscriptionErrors.RefundAlreadyProcessed(providerTxId));
        }

        RefundRequest refundRequest = null!;

        await _unitOfWork.ExecuteAsync(async () =>
        {
            // 1. Revoke subscription immediately
            subscription.Tier = SubscriptionTier.Free;
            subscription.Status = SubscriptionStatus.Refunded;
            subscription.ExpiresAtUtc = DateTime.UtcNow;
            subscription.AutoRenew = false;
            _subscriptionRepo.Update(subscription);

            // 2. Clear Redis Tier
            await _tierCache.InvalidateTierAsync(subscription.UserId, cancellationToken);

            // 3. Insert Refund Request state row
            refundRequest = new RefundRequest
            {
                UserId = request.UserId,
                PaymentAttemptId = attempt.Id,
                ProviderTransactionId = providerTxId,
                AmountCents = attempt.AmountCents,
                Currency = attempt.Currency,
                Reason = request.Reason,
                RefundType = RefundType.ExplicitSubscriptionRefund,
                Status = RefundRequestStatus.Pending
            };
            await _refundRepo.AddAsync(refundRequest, cancellationToken);

            // 4. Stage Outbox Events (Refund execution + Subscription expired)
            await _publishEndpoint.Publish(new ExecutePaymobRefundCommandEvent(
                refundRequest.Id,
                request.UserId,
                attempt.Id,
                providerTxId,
                attempt.AmountCents,
                attempt.Currency,
                request.Reason), cancellationToken);

            await _publishEndpoint.Publish(new SubscriptionExpiredEvent(
                request.UserId,
                DateTime.UtcNow), cancellationToken);
        }, cancellationToken);

        _logger.LogInformation("Staged explicit refund for user {UserId}, amount: {Amount} {Currency}, tx: {TxId}",
            request.UserId, attempt.AmountCents / 100m, attempt.Currency, providerTxId);

        return Result<RefundSubscriptionResponse>.Success(new RefundSubscriptionResponse(
            refundRequest.Id,
            request.UserId,
            providerTxId,
            attempt.AmountCents / 100m,
            attempt.Currency,
            RefundRequestStatus.Pending.ToString(),
            "Refund has been initiated and staged for Paymob processing."));
    }
}
