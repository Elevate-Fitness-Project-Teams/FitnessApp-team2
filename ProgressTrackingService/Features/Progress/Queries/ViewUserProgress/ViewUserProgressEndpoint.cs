using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Extensions;
using System.Security.Claims;

namespace ProgressTrackingService.Features.Progress.Queries.ViewUserProgress;

public static class ViewUserProgressEndpoint
{
    public static void MapViewUserProgressEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/progress/{userId:guid}", async (
            [FromRoute] Guid userId,
            HttpContext httpContext,
            ISender sender) =>
        {
            var callerIdStr = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(callerIdStr) || !Guid.TryParse(callerIdStr, out var callerId))
            {
                return Results.Unauthorized();
            }

            // Since only the user can view their own progress, let's enforce authorization (caller == userId)
            // Or if an admin role exists. For now, assuming caller must match userId for basic auth logic.
            // If the criteria implies any authenticated user can view, we just proceed.
            // The story says: "Role: Authenticated. Given userId exists and the caller is authorized to view it."
            // So we'll enforce callerId == userId for authorization.
            if (callerId != userId)
            {
                return Results.Forbid();
            }

            var query = new ViewUserProgressOrchestrator(userId);
            var result = await sender.Send(query);

            if (!result.IsSuccess)
            {
                return result.ToHttpResult();
            }

            return Results.Ok(ApiResponse<ViewUserProgressResponse>.Success(result.Value));
        })
        .RequireAuthorization()
        .WithName("ViewUserProgress")
        .WithTags("ProgressTracking");
    }
}
