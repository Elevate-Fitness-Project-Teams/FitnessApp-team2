using FitnessCalculationService.Common;
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
            
            if (result.IsSuccess)
            {
                var response = ApiResponse<Guid>.Success(result.Value, "Created successfully.", StatusCodes.Status201Created);
                return Results.Created($"/api/v1/fitness/stats/{command.UserId}", response);
            }
            
            var errorResponse = ApiResponse<Guid>.Failure(result.Errors.Select(e => e.Message), result.Error?.Message ?? "Failed to process request", StatusCodes.Status400BadRequest);
            return Results.BadRequest(errorResponse);
        })
        .WithName("SubmitFitnessStats")
        .Produces<ApiResponse<Guid>>(StatusCodes.Status201Created)
        .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);
    }
}
