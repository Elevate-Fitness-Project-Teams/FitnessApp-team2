using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Extensions;
using ProgressTrackingService.Common.Security;

namespace ProgressTrackingService.Features.Progress.Queries.GetProgressDashboard;

public static class GetProgressDashboardEndpoint
{
    public static void MapGetProgressDashboardEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/progress", async (
            [FromQuery] string? period,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            HttpContext httpContext,
            IMediator mediator) =>
        {
            var userId = httpContext.GetUserId();
            if (!userId.HasValue)
            {
                return Results.Unauthorized();
            }

            var query = new GetProgressDashboardQuery(
                userId.Value,
                period,
                startDate,
                endDate);

            var result = await mediator.Send(query);
            return result.ToHttpResult();
        })
        .RequireAuthorization()
        .WithName("GetProgressDashboard")
        .WithTags("ProgressTracking")
        .Produces<ApiResponse<ProgressDashboardResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithSummary("Retrieve comprehensive progress review dashboard with period comparisons and trends");
    }
}
