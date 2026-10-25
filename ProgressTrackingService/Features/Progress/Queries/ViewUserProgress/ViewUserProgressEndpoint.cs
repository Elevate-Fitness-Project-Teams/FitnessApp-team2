using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Errors;
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
            IMediator mediator) =>
        {
            var callerIdStr = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(callerIdStr) || !Guid.TryParse(callerIdStr, out var callerId))
            {
                return Results.Json(
                    ApiResponse<object?>.Failure(new[] { ProgressErrors.AuthTokenInvalid }, "Unauthorized.", 401),
                    statusCode: 401);
            }

            var isAdmin = httpContext.User.IsInRole("Admin");
            if (callerId != userId && !isAdmin)
            {
                return Results.Json(
                    ApiResponse<object?>.Failure(new[] { ProgressErrors.PermForbidden }, "You are not authorized to view this user's progress.", 403),
                    statusCode: 403);
            }

            var query = new ViewUserProgressOrchestrator(userId);
            var result = await mediator.Send(query);

            return result.ToHttpResult();
        })
        .RequireAuthorization()
        .WithName("ViewUserProgress")
        .WithTags("ProgressTracking");
    }
}
