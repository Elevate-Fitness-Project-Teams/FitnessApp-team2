using FitnessCalculationService.Common;
using FitnessCalculationService.Common.Http;
using FitnessCalculationService.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCalculationService.Features.Calculations.Commands.RecalculateMetrics;

public static class RecalculateMetricsEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/v1/fitness/recalculate/{userId}", async (
            [FromRoute] Guid userId,
            [FromBody] RecalculateMetricsRequest? request,
            ICurrentUser currentUser,
            IMediator mediator) =>
        {
            var command = new RecalculateMetricsOrchestrator(
                userId,
                request?.NewWeight,
                request?.Reason,
                "user",
                DateTime.UtcNow
            );

            var result = await mediator.Send(command);
            return result.ToHttpResult("Metrics recalculated successfully.", StatusCodes.Status200OK);
        })
        .RequireAuthorization()
        .AddEndpointFilter(new OwnershipFilter("userId"))
        .WithName("RecalculateMetrics")
        .WithTags("Metrics")
        .WithSummary("Recalculate metabolic metrics and update plan")
        .WithDescription("Recomputes user metabolic targets upon weight change. If caloric status transitions across tier boundaries, automatically deactivates prior plan and assigns new matching plan.")
        .Produces<ApiResponse<RecalculateMetricsResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
        .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
        .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);
    }
}

