using FitnessCalculationService.Common;
using FitnessCalculationService.Common.Http;
using FitnessCalculationService.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCalculationService.Features.Calculations.Commands.RecalculateMetrics;

public record RecalculateMetricsRequest(double? NewWeight = null, string? Reason = null);

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
        .Produces<ApiResponse<RecalculateMetricsResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
        .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
        .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);
    }
}
