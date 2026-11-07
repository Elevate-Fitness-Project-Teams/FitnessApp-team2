using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using SubscriptionBillingService.Common;
using SubscriptionBillingService.Common.Database;
using SubscriptionBillingService.Common.Errors;
using SubscriptionBillingService.Contracts;
using SubscriptionBillingService.Domain.Entities;
using SubscriptionBillingService.Domain.Enums;
using SubscriptionBillingService.Features.Subscriptions.Commands.ProcessPaymobWebhook;
using SubscriptionBillingService.Infrastructure.Caching;
using SubscriptionBillingService.Infrastructure.Paymob;

namespace SubscriptionBillingService.Features.Subscriptions.Orchestrators;

public class ProcessPaymobWebhookOrchestrator : IRequestHandler<ProcessPaymobWebhookCommand, Result<string>>
{
    private readonly PaymobHmacValidator _hmacValidator;
    private readonly IGenericRepository<UserSubscription> _subscriptionRepo;
    private readonly IGenericRepository<PaymentAttempt> _attemptRepo;
    private readonly IGenericRepository<BillingLog> _billingLogRepo;
    private readonly IGenericRepository<RefundRequest> _refundRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ISubscriptionTierCache _tierCache;
    private readonly ILogger<ProcessPaymobWebhookOrchestrator> _logger;

    public ProcessPaymobWebhookOrchestrator(
        PaymobHmacValidator hmacValidator,
        IGenericRepository<UserSubscription> subscriptionRepo,
        IGenericRepository<PaymentAttempt> attemptRepo,
        IGenericRepository<BillingLog> billingLogRepo,
        IGenericRepository<RefundRequest> refundRepo,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint,
        ISubscriptionTierCache tierCache,
        ILogger<ProcessPaymobWebhookOrchestrator> logger)
    {
        _hmacValidator = hmacValidator;
        _subscriptionRepo = subscriptionRepo;
        _attemptRepo = attemptRepo;
        _billingLogRepo = billingLogRepo;
        _refundRepo = refundRepo;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
        _tierCache = tierCache;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(
        ProcessPaymobWebhookCommand request,
        CancellationToken cancellationToken)
    {
        var obj = request.Payload.Obj;
        var hmac = request.Hmac;

        // 1. Validate HMAC signature
        if (!_hmacValidator.ValidateHmac(obj, hmac))
        {
            _logger.LogWarning("Paymob webhook signature validation failed for transaction {TxId}", obj.Id);
            return Result<string>.Failure(SubscriptionErrors.InvalidSignature());
        }

        var providerTxId = obj.Id.ToString();

        // 2. Check early idempotency against BillingLogs
        var existingLog = await _billingLogRepo.FirstOrDefaultAsync(
            b => b.ProviderTransactionId == providerTxId, cancellationToken);

        if (existingLog != null)
        {
            _logger.LogInformation("Webhook for transaction {TxId} was already processed. Returning idempotent OK.", providerTxId);
            return Result<string>.Success("Transaction already processed.");
        }

        // 3. Locate PaymentAttempt
        PaymentAttempt? attempt = null;

        var specialRef = obj.SpecialReference ?? obj.Order?.SpecialReference;
        if (!string.IsNullOrWhiteSpace(specialRef) && Guid.TryParse(specialRef, out var attemptId))
        {
            attempt = await _attemptRepo.GetByIdAsync(attemptId, cancellationToken);
        }

        if (attempt == null && obj.Order?.Id != null)
        {
            attempt = await _attemptRepo.FirstOrDefaultAsync(
                a => a.ProviderOrderId == obj.Order.Id.ToString(), cancellationToken);
        }

        if (attempt == null)
        {
            _logger.LogWarning("No matching PaymentAttempt found for Paymob transaction {TxId}", providerTxId);
        }

        var userId = attempt?.UserId ?? Guid.Empty;
        var planTier = attempt?.PlanTier ?? SubscriptionTier.Premium;
        var durationMonths = attempt?.DurationMonths ?? 1;

        // 4. Atomic Execution inside UnitOfWork
        await _unitOfWork.ExecuteAsync(async () =>
        {
            // Re-check inside transaction lock
            var alreadyRecorded = await _billingLogRepo.FirstOrDefaultAsync(
                b => b.ProviderTransactionId == providerTxId, cancellationToken);
            if (alreadyRecorded != null)
            {
                return;
            }

            if (obj.Success)
            {
                // Check if user already has an active subscription
                UserSubscription? userSub = null;
                if (userId != Guid.Empty)
                {
                    userSub = await _subscriptionRepo.FirstOrDefaultAsync(
                        s => s.UserId == userId, cancellationToken);
                }

                var isAlreadyActive = userSub != null &&
                                      userSub.Tier == SubscriptionTier.Premium &&
                                      userSub.Status == SubscriptionStatus.Active &&
                                      userSub.ExpiresAtUtc.HasValue &&
                                      userSub.ExpiresAtUtc.Value > DateTime.UtcNow;

                if (isAlreadyActive && attempt != null)
                {
                    // === ATOMIC AUTO-REFUND SAGA ===
                    _logger.LogWarning("Duplicate charge detected for user {UserId} (TxId: {TxId}). Staging automatic refund!",
                        userId, providerTxId);

                    attempt.Status = PaymentAttemptStatus.DuplicateChargePendingRefund;
                    attempt.ProviderTransactionId = providerTxId;
                    attempt.CompletedAtUtc = DateTime.UtcNow;
                    _attemptRepo.Update(attempt);

                    var duplicateBillingLog = new BillingLog
                    {
                        UserId = userId,
                        PaymentAttemptId = attempt.Id,
                        PlanTier = planTier,
                        DurationMonths = durationMonths,
                        Amount = obj.AmountCents / 100m,
                        Currency = obj.Currency,
                        PaymentProvider = "Paymob",
                        ProviderTransactionId = providerTxId,
                        ProviderIntentionId = attempt.ProviderIntentionId,
                        PaymentStatus = BillingPaymentStatus.DuplicateChargeDetected,
                        FailureReason = "Duplicate charge detected: user already holds active Premium. Auto-refund initiated."
                    };
                    await _billingLogRepo.AddAsync(duplicateBillingLog, cancellationToken);

                    var refundRequest = new RefundRequest
                    {
                        UserId = userId,
                        PaymentAttemptId = attempt.Id,
                        ProviderTransactionId = providerTxId,
                        AmountCents = (int)obj.AmountCents,
                        Currency = obj.Currency,
                        Reason = "DuplicateActiveSubscription",
                        RefundType = RefundType.AutoDuplicateRefund,
                        Status = RefundRequestStatus.Pending
                    };
                    await _refundRepo.AddAsync(refundRequest, cancellationToken);

                    // Stage Outbox command for the Paymob refund consumer
                    await _publishEndpoint.Publish(new ExecutePaymobRefundCommandEvent(
                        refundRequest.Id,
                        userId,
                        attempt.Id,
                        providerTxId,
                        (int)obj.AmountCents,
                        obj.Currency,
                        "DuplicateActiveSubscription"), cancellationToken);
                }
                else
                {
                    // === NORMAL SUCCESSFUL UPGRADE ===
                    if (attempt != null)
                    {
                        attempt.Status = PaymentAttemptStatus.Succeeded;
                        attempt.ProviderTransactionId = providerTxId;
                        attempt.CompletedAtUtc = DateTime.UtcNow;
                        _attemptRepo.Update(attempt);
                    }

                    var billingLog = new BillingLog
                    {
                        UserId = userId,
                        PaymentAttemptId = attempt?.Id ?? Guid.Empty,
                        PlanTier = planTier,
                        DurationMonths = durationMonths,
                        Amount = obj.AmountCents / 100m,
                        Currency = obj.Currency,
                        PaymentProvider = "Paymob",
                        ProviderTransactionId = providerTxId,
                        ProviderIntentionId = attempt?.ProviderIntentionId,
                        PaymentStatus = BillingPaymentStatus.Success
                    };
                    await _billingLogRepo.AddAsync(billingLog, cancellationToken);

                    if (userId != Guid.Empty)
                    {
                        var expiresAt = DateTime.UtcNow.AddMonths(durationMonths);

                        if (userSub == null)
                        {
                            userSub = new UserSubscription
                            {
                                UserId = userId,
                                Tier = SubscriptionTier.Premium,
                                Status = SubscriptionStatus.Active,
                                PlanDurationMonths = durationMonths,
                                AutoRenew = true,
                                StartedAtUtc = DateTime.UtcNow,
                                ExpiresAtUtc = expiresAt,
                                LastProviderTransactionId = providerTxId
                            };
                            await _subscriptionRepo.AddAsync(userSub, cancellationToken);
                        }
                        else
                        {
                            userSub.Tier = SubscriptionTier.Premium;
                            userSub.Status = SubscriptionStatus.Active;
                            userSub.PlanDurationMonths = durationMonths;
                            userSub.AutoRenew = true;
                            userSub.StartedAtUtc = DateTime.UtcNow;
                            userSub.ExpiresAtUtc = expiresAt;
                            userSub.LastProviderTransactionId = providerTxId;
                            userSub.CancelledAtUtc = null;
                            _subscriptionRepo.Update(userSub);
                        }

                        // Stage MassTransit Outbox Event
                        await _publishEndpoint.Publish(new SubscriptionUpgradedEvent(
                            userId,
                            SubscriptionTier.Premium.ToString(),
                            durationMonths,
                            DateTime.UtcNow,
                            expiresAt,
                            providerTxId), cancellationToken);

                        // Cache in Redis for API Gateway dynamic rate limit
                        await _tierCache.SetTierAsync(
                            userId,
                            SubscriptionTier.Premium.ToString(),
                            expiresAt - DateTime.UtcNow,
                            cancellationToken);
                    }
                }
            }
            else
            {
                // === PAYMENT FAILED ===
                if (attempt != null)
                {
                    attempt.Status = PaymentAttemptStatus.Failed;
                    attempt.ProviderTransactionId = providerTxId;
                    attempt.FailureReason = "Payment failed at Paymob provider.";
                    _attemptRepo.Update(attempt);
                }

                var failedLog = new BillingLog
                {
                    UserId = userId,
                    PaymentAttemptId = attempt?.Id ?? Guid.Empty,
                    PlanTier = planTier,
                    DurationMonths = durationMonths,
                    Amount = obj.AmountCents / 100m,
                    Currency = obj.Currency,
                    PaymentProvider = "Paymob",
                    ProviderTransactionId = providerTxId,
                    PaymentStatus = BillingPaymentStatus.Failed,
                    FailureReason = "Paymob callback reported transaction failure."
                };
                await _billingLogRepo.AddAsync(failedLog, cancellationToken);
            }
        }, cancellationToken);

        return Result<string>.Success("Webhook processed successfully.");
    }
}
