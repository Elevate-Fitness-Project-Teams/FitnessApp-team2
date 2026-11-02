using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Extensions;
using ProgressTrackingService.Common.Security;
using ProgressTrackingService.Features.Achievements.DTOs;

namespace ProgressTrackingService.Features.Achievements.Queries.GetUserAchievements;

public static class GetUserAchievementsEndpoint
{
    public static void MapGetUserAchievementsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/progress/achievements", async (
            [FromQuery] bool? includeLocked,
            HttpContext httpContext,
            IMediator mediator) =>
        {
            var userId = httpContext.GetUserId();
            if (!userId.HasValue)
            {
                return Results.Unauthorized();
            }

            var query = new GetUserAchievementsQuery(userId.Value, includeLocked ?? false);
            var result = await mediator.Send(query);
            return result.ToHttpResult();
        })
        .RequireAuthorization()
        .WithName("GetUserAchievements")
        .WithTags("Achievements")
        .Produces<ApiResponse<List<UserAchievementDto>>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithSummary("Retrieve unlocked achievements and optionally locked milestone badges for the caller");
    }
}
