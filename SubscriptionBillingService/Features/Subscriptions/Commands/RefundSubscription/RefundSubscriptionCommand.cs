using MediatR;
using SubscriptionBillingService.Common;

namespace SubscriptionBillingService.Features.Subscriptions.Commands.RefundSubscription;

public record RefundSubscriptionCommand(
    Guid UserId,
    string Reason) : IRequest<Result<RefundSubscriptionResponse>>;

public record RefundSubscriptionResponse(
    Guid RefundRequestId,
    Guid UserId,
    string ProviderTransactionId,
    decimal Amount,
    string Currency,
    string Status,
    string Message);
