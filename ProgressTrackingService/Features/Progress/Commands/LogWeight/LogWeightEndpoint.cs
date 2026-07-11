using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Extensions;
using System.Security.Claims;
using ProgressTrackingService.Features.Progress.Orchestrators;

namespace ProgressTrackingService.Features.Progress.Commands.LogWeight;

public static class LogWeightEndpoint
{
    public static void MapLogWeightEndpoint(this IEndpointRouteBuilder app)
    {
		_ = app.MapPost("/api/v1/progress/weight", async (
			[FromBody] LogWeightRequest request,
			HttpContext httpContext,
			ISender sender) =>
		{
			var userIdStr = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
			{
				return Results.Unauthorized();
			}

			var orchestrator = new LogWeightOrchestrator(
				userId,
				request.Weight,
				request.Date,
				request.Notes
			);

			var result = await sender.Send(orchestrator);

			if (!result.IsSuccess)
			{
				return result.ToHttpResult();
			}

			return result.ToHttpResult();
		})
		.RequireAuthorization()
		.WithName("LogWeight")
		.WithTags("ProgressTracking");
    }
}
