using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Extensions;
using ProgressTrackingService.Common.Security;

namespace ProgressTrackingService.Features.Progress.Queries.ViewUserProgress;

public static class ViewUserProgressEndpoint
{
    public static void MapViewUserProgressEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/progress/{userId:guid}", async (
            [FromRoute] Guid userId,
            HttpContext httpContext,
            IMediator mediator) =>
        {
            var accessError = httpContext.EnsureAccess(userId);
            if (accessError != null)
            {
                return accessError;
            }

            var query = new ViewUserProgressOrchestrator(userId);
            var result = await mediator.Send(query);

            return result.ToHttpResult();
        })
        .RequireAuthorization()
        .WithName("ViewUserProgress")
        .WithTags("ProgressTracking")
        .Produces<ApiResponse<ViewUserProgressResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Retrieve user progress profile with bounded recent workout and weight activity");
    }
}
