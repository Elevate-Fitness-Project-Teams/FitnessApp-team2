using MediatR;
using SubscriptionBillingService.Common;
using SubscriptionBillingService.Infrastructure.Paymob.Models;

namespace SubscriptionBillingService.Features.Subscriptions.Commands.ProcessPaymobWebhook;

public record ProcessPaymobWebhookCommand(
    PaymobWebhookPayload Payload,
    string? Hmac) : IRequest<Result<string>>;
