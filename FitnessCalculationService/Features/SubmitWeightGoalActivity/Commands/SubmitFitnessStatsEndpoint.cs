using FitnessCalculationService.Common;
using FitnessCalculationService.Common.Http;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCalculationService.Features.SubmitWeightGoalActivity.Commands;

public static class SubmitFitnessStatsEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/fitness/weight-goal-activity", async ([FromBody] SubmitFitnessStatsCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.ToHttpResult("Created successfully.", StatusCodes.Status201Created, $"/api/v1/fitness/stats/{command.UserId}");
        })
        .RequireAuthorization()
        .WithName("SubmitFitnessStats")
        .Produces<ApiResponse<Guid>>(StatusCodes.Status201Created)
        .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
        .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);
    }
}
