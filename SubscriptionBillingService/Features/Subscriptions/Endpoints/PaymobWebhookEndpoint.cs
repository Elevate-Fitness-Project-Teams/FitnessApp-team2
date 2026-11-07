using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubscriptionBillingService.Common;
using SubscriptionBillingService.Features.Subscriptions.Commands.ProcessPaymobWebhook;
using SubscriptionBillingService.Infrastructure.Paymob.Models;

namespace SubscriptionBillingService.Features.Subscriptions.Endpoints;

[ApiController]
[Route("api/v1/subscription/webhooks")]
public class PaymobWebhookEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymobWebhookEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("paymob")]
    [AllowAnonymous]
    public async Task<IActionResult> ProcessWebhook(
        [FromQuery] string? hmac,
        [FromBody] PaymobWebhookPayload payload,
        CancellationToken cancellationToken)
    {
        var command = new ProcessPaymobWebhookCommand(payload, hmac);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Type == ErrorType.Unauthorized)
            {
                return Unauthorized(ApiResponse<string>.Failure(
                    result.Error.Description,
                    result.Error.Code,
                    StatusCodes.Status401Unauthorized));
            }

            return BadRequest(ApiResponse<string>.Failure(
                result.Error.Description,
                result.Error.Code,
                StatusCodes.Status400BadRequest));
        }

        return Ok(ApiResponse<string>.Success(
            result.Value,
            "Webhook processed successfully."));
    }
}
