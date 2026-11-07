using MassTransit;
using Microsoft.Extensions.Logging;
using SubscriptionBillingService.Common.Database;
using SubscriptionBillingService.Contracts;
using SubscriptionBillingService.Domain.Entities;
using SubscriptionBillingService.Domain.Enums;
using SubscriptionBillingService.Infrastructure.Paymob;
using SubscriptionBillingService.Infrastructure.Paymob.Models;

namespace SubscriptionBillingService.Features.Subscriptions.Consumers;

public class ProcessPaymobRefundConsumer : IConsumer<ExecutePaymobRefundCommandEvent>
{
    private readonly IPaymobGatewayClient _paymobGatewayClient;
    private readonly IGenericRepository<RefundRequest> _refundRepo;
    private readonly IGenericRepository<PaymentAttempt> _attemptRepo;
    private readonly IGenericRepository<BillingLog> _billingLogRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessPaymobRefundConsumer> _logger;

    public ProcessPaymobRefundConsumer(
        IPaymobGatewayClient paymobGatewayClient,
        IGenericRepository<RefundRequest> refundRepo,
        IGenericRepository<PaymentAttempt> attemptRepo,
        IGenericRepository<BillingLog> billingLogRepo,
        IUnitOfWork unitOfWork,
        ILogger<ProcessPaymobRefundConsumer> logger)
    {
        _paymobGatewayClient = paymobGatewayClient;
        _refundRepo = refundRepo;
        _attemptRepo = attemptRepo;
        _billingLogRepo = billingLogRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ExecutePaymobRefundCommandEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Processing refund command for RefundRequestId: {RefundId}, ProviderTxId: {TxId}, Amount: {Amount}",
            msg.RefundRequestId, msg.ProviderTransactionId, msg.AmountCents);

        if (!long.TryParse(msg.ProviderTransactionId, out var transactionId))
        {
            _logger.LogError("Invalid ProviderTransactionId '{TxId}' - cannot parse to long for Paymob API.", msg.ProviderTransactionId);
            await MarkRefundFailedAsync(msg.RefundRequestId, $"Invalid transaction id format: {msg.ProviderTransactionId}");
            return;
        }

        try
        {
            var refundResponse = await _paymobGatewayClient.RefundTransactionAsync(new PaymobRefundRequest
            {
                TransactionId = transactionId,
                AmountCents = msg.AmountCents
            }, context.CancellationToken);

            if (refundResponse.Success || refundResponse.IsRefund)
            {
                await _unitOfWork.ExecuteAsync(async () =>
                {
                    var refund = await _refundRepo.GetByIdAsync(msg.RefundRequestId, context.CancellationToken);
                    if (refund != null)
                    {
                        refund.Status = RefundRequestStatus.Refunded;
                        refund.ProviderRefundTransactionId = refundResponse.Id.ToString();
                        refund.CompletedAtUtc = DateTime.UtcNow;
                        _refundRepo.Update(refund);
                    }

                    var attempt = await _attemptRepo.GetByIdAsync(msg.PaymentAttemptId, context.CancellationToken);
                    if (attempt != null)
                    {
                        attempt.Status = attempt.Status == PaymentAttemptStatus.DuplicateChargePendingRefund
                            ? PaymentAttemptStatus.AutoRefunded
                            : PaymentAttemptStatus.Refunded;
                        _attemptRepo.Update(attempt);
                    }

                    var refundBillingLog = new BillingLog
                    {
                        UserId = msg.UserId,
                        PaymentAttemptId = msg.PaymentAttemptId,
                        PlanTier = attempt?.PlanTier ?? SubscriptionTier.Premium,
                        DurationMonths = attempt?.DurationMonths ?? 1,
                        Amount = -(msg.AmountCents / 100m),
                        Currency = msg.Currency,
                        PaymentProvider = "Paymob",
                        ProviderTransactionId = $"{msg.ProviderTransactionId}:refund",
                        ProviderIntentionId = attempt?.ProviderIntentionId,
                        PaymentStatus = BillingPaymentStatus.Refunded,
                        FailureReason = $"Refunded reason: {msg.Reason}"
                    };
                    await _billingLogRepo.AddAsync(refundBillingLog, context.CancellationToken);

                    await context.Publish(new PaymentRefundedEvent(
                        msg.RefundRequestId,
                        msg.UserId,
                        msg.PaymentAttemptId,
                        msg.ProviderTransactionId,
                        refundResponse.Id.ToString(),
                        msg.AmountCents,
                        msg.Currency,
                        msg.Reason,
                        DateTime.UtcNow), context.CancellationToken);
                }, context.CancellationToken);

                _logger.LogInformation("Successfully completed refund for RefundRequestId: {RefundId}, PaymobRefundTxId: {RefundTxId}",
                    msg.RefundRequestId, refundResponse.Id);
            }
            else
            {
                _logger.LogWarning("Paymob returned non-success for refund {RefundId}", msg.RefundRequestId);
                await IncrementRetryAsync(msg.RefundRequestId, "Paymob refund response indicated non-success.");
                throw new InvalidOperationException($"Paymob refund returned non-success for tx {transactionId}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while executing Paymob refund for tx {TxId}", msg.ProviderTransactionId);
            await IncrementRetryAsync(msg.RefundRequestId, ex.Message);
            throw; // Re-throw to allow MassTransit exponential retry policy
        }
    }

    private async Task MarkRefundFailedAsync(Guid refundRequestId, string error)
    {
        await _unitOfWork.ExecuteAsync(async () =>
        {
            var refund = await _refundRepo.GetByIdAsync(refundRequestId);
            if (refund != null)
            {
                refund.Status = RefundRequestStatus.FailedManualIntervention;
                refund.LastError = error;
                _refundRepo.Update(refund);
            }
        });
    }

    private async Task IncrementRetryAsync(Guid refundRequestId, string error)
    {
        try
        {
            await _unitOfWork.ExecuteAsync(async () =>
            {
                var refund = await _refundRepo.GetByIdAsync(refundRequestId);
                if (refund != null)
                {
                    refund.RetryCount++;
                    refund.LastError = error;
                    if (refund.RetryCount >= 5)
                    {
                        refund.Status = RefundRequestStatus.FailedManualIntervention;
                    }
                    _refundRepo.Update(refund);
                }
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to record refund retry for {RefundId}", refundRequestId);
        }
    }
}
