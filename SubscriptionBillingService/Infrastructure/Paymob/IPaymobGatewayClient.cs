using SubscriptionBillingService.Infrastructure.Paymob.Models;

namespace SubscriptionBillingService.Infrastructure.Paymob;

public interface IPaymobGatewayClient
{
    Task<PaymobIntentionResponse> CreateIntentionAsync(PaymobIntentionRequest request, CancellationToken cancellationToken = default);
    Task<PaymobRefundResponse> RefundTransactionAsync(PaymobRefundRequest request, CancellationToken cancellationToken = default);
    Task<PaymobTransactionObject> GetTransactionByIdAsync(long transactionId, CancellationToken cancellationToken = default);
}
