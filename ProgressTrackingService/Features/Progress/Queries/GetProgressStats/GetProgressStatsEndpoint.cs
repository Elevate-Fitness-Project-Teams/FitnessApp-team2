using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Extensions;
using ProgressTrackingService.Common.Security;

namespace ProgressTrackingService.Features.Progress.Queries.GetProgressStats;

public static class GetProgressStatsEndpoint
{
    public static void MapGetProgressStatsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/progress/stats/{userId:guid}", async (
            [FromRoute] Guid userId,
            HttpContext httpContext,
            IMediator mediator) =>
        {
            var accessError = httpContext.EnsureAccess(userId);
            if (accessError != null)
            {
                return accessError;
            }

            var query = new GetProgressStatsQuery(userId);
            var result = await mediator.Send(query);
            return result.ToHttpResult();
        })
        .RequireAuthorization()
        .WithName("GetProgressStats")
        .WithTags("ProgressTracking")
        .Produces<ApiResponse<ProgressStatsDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Retrieve user progress statistics and effective streak summary");
    }
}
