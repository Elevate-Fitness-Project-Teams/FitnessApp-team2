using FitnessCalculationService.Common;
using FitnessCalculationService.Common.Http;
using FitnessCalculationService.Common.Security;
using FitnessCalculationService.Features.CalculateFitnessMetrics.Response;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCalculationService.Features.CalculateFitnessMetrics.Commands;

public static class CalculateFitnessMetricsEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/fitness/calculate", async ([FromBody] CalculateFitnessMetricsOrchstrator command, ICurrentUser currentUser, IMediator mediator) =>
        {
            var effectiveUserId = string.IsNullOrWhiteSpace(command.UserId) && currentUser.UserId.HasValue
                ? currentUser.UserId.Value.ToString()
                : command.UserId;

            var result = await mediator.Send(command with { UserId = effectiveUserId });
            return result.ToHttpResult("Calculated successfully.", StatusCodes.Status200OK);
        })
        .RequireAuthorization()
        .AddEndpointFilter(new OwnershipFilter("userId"))
        .WithName("CalculateFitnessMetrics")
        .Produces<ApiResponse<CalculateFitnessMetricsResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
        .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
        .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);
    }
}
