using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Extensions;
using ProgressTrackingService.Common.Pagination;
using ProgressTrackingService.Common.Security;

namespace ProgressTrackingService.Features.Progress.Queries.GetWeightHistory;

public static class GetWeightHistoryEndpoint
{
    public static void MapGetWeightHistoryEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/progress/weight-history/{userId:guid}", async (
            [FromRoute] Guid userId,
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            HttpContext httpContext,
            IMediator mediator) =>
        {
            var accessError = httpContext.EnsureAccess(userId);
            if (accessError != null)
            {
                return accessError;
            }

            var query = new GetWeightHistoryQuery(
                userId,
                from,
                to,
                page ?? 1,
                pageSize ?? 20);

            var result = await mediator.Send(query);
            return result.ToHttpResult();
        })
        .RequireAuthorization()
        .WithName("GetWeightHistory")
        .WithTags("ProgressTracking")
        .Produces<ApiResponse<PagedResult<WeightHistoryDto>>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .WithSummary("Retrieve paginated chronological weight history for a user");
    }
}
