using MediatR;
using Microsoft.AspNetCore.Mvc;
using SubscriptionBillingService.Common;
using SubscriptionBillingService.Features.Subscriptions.Commands.RefundSubscription;

namespace SubscriptionBillingService.Features.Subscriptions.Endpoints;

public record RefundSubscriptionRequest(Guid UserId, string Reason);

[ApiController]
[Route("api/v1/subscription")]
public class RefundSubscriptionEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public RefundSubscriptionEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("refund")]
    public async Task<IActionResult> Refund(
        [FromBody] RefundSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new RefundSubscriptionCommand(request.UserId, request.Reason),
            cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            return StatusCode(
                statusCode,
                ApiResponse<RefundSubscriptionResponse>.Failure(result.Error.Description, result.Error.Code, statusCode));
        }

        return Ok(ApiResponse<RefundSubscriptionResponse>.Success(
            result.Value,
            "Refund initiated successfully."));
    }
}
