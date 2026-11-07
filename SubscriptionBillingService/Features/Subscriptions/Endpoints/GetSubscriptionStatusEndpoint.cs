using MediatR;
using Microsoft.AspNetCore.Mvc;
using SubscriptionBillingService.Common;
using SubscriptionBillingService.Features.Subscriptions.Queries.GetSubscriptionStatus;

namespace SubscriptionBillingService.Features.Subscriptions.Endpoints;

[ApiController]
[Route("api/v1/subscription")]
public class GetSubscriptionStatusEndpoint : ControllerBase
{
    private readonly IMediator _mediator;

    public GetSubscriptionStatusEndpoint(IMediator _mediator)
    {
        this._mediator = _mediator;
    }

    [HttpGet("status/{userId:guid}")]
    public async Task<IActionResult> GetStatus([FromRoute] Guid userId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSubscriptionStatusQuery(userId), cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode(
                result.Error.Type == ErrorType.NotFound ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest,
                ApiResponse<GetSubscriptionStatusResponse>.Failure(result.Error.Description, result.Error.Code, 400));
        }

        return Ok(ApiResponse<GetSubscriptionStatusResponse>.Success(
            result.Value,
            "Subscription status retrieved successfully."));
    }
}
