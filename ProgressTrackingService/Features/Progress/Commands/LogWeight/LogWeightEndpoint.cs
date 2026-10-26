using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Extensions;
using System.Security.Claims;
using ProgressTrackingService.Common.Security;
using ProgressTrackingService.Features.Progress.Orchestrators;

namespace ProgressTrackingService.Features.Progress.Commands.LogWeight;

public static class LogWeightEndpoint
{
    public static void MapLogWeightEndpoint(this IEndpointRouteBuilder app)
    {
		_ = app.MapPost("/api/v1/progress/weight", async (
			[FromBody] LogWeightRequest request,
			HttpContext httpContext,
			IMediator mediator) =>
		{
			var userId = httpContext.GetUserId();
			if (!userId.HasValue)
			{
				return Results.Unauthorized();
			}

			var orchestrator = new LogWeightOrchestrator(
				userId.Value,
				request.Weight,
				request.Date,
				request.Notes
			);

			var result = await mediator.Send(orchestrator);

			if (!result.IsSuccess)
			{
				return result.ToHttpResult();
			}

			return Results.Created(
				$"/api/v1/progress/weight/{result.Value.Id}",
				ApiResponse<LogWeightResponse>.Success(result.Value));
		})
		.RequireAuthorization()
		.WithName("LogWeight")
		.WithTags("ProgressTracking");
    }
}
