using MediatR;
using Microsoft.AspNetCore.Mvc;
using SubscriptionBillingService.Common;
using SubscriptionBillingService.Domain.Enums;
using SubscriptionBillingService.Features.Subscriptions.Commands.UpgradeSubscription;

namespace SubscriptionBillingService.Features.Subscriptions.Endpoints;

public record UpgradeSubscriptionRequest(
    Guid UserId,
    SubscriptionTier PlanTier,
    int DurationMonths);

[ApiController]
[Route("api/v1/subscription")]
public class UpgradeSubscriptionEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public UpgradeSubscriptionEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("upgrade")]
    public async Task<IActionResult> Upgrade(
        [FromBody] UpgradeSubscriptionRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var command = new UpgradeSubscriptionCommand(
            request.UserId,
            request.PlanTier,
            request.DurationMonths,
            idempotencyKey);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Type switch
            {
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Unavailable => StatusCodes.Status502BadGateway,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            return StatusCode(
                statusCode,
                ApiResponse<UpgradeSubscriptionResponse>.Failure(result.Error.Description, result.Error.Code, statusCode));
        }

        return Ok(ApiResponse<UpgradeSubscriptionResponse>.Success(
            result.Value,
            "Paymob checkout session created successfully."));
    }
}
