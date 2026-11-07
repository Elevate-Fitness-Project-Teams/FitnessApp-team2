using MediatR;
using Microsoft.AspNetCore.Mvc;
using SubscriptionBillingService.Common;
using SubscriptionBillingService.Features.Subscriptions.Commands.CancelSubscription;

namespace SubscriptionBillingService.Features.Subscriptions.Endpoints;

public record CancelSubscriptionRequest(Guid UserId);

[ApiController]
[Route("api/v1/subscription")]
public class CancelSubscriptionEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public CancelSubscriptionEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("cancel")]
    public async Task<IActionResult> Cancel(
        [FromBody] CancelSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CancelSubscriptionCommand(request.UserId), cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type == ErrorType.NotFound
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return StatusCode(
                statusCode,
                ApiResponse<CancelSubscriptionResponse>.Failure(result.Error.Description, result.Error.Code, statusCode));
        }

        return Ok(ApiResponse<CancelSubscriptionResponse>.Success(
            result.Value,
            "Subscription auto-renewal turned off successfully. Paid access continues until expiration."));
    }
}
