using FitnessCalculationService.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using FitnessCalculationService.Features.CalculateFitnessMetrics.Response;

namespace FitnessCalculationService.Features.CalculateFitnessMetrics.Commands;

public static class CalculateFitnessMetricsEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/fitness/calculate", async ([FromBody] CalculateFitnessMetricsOrchstrator command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            
            if (result.IsSuccess)
            {
                var response = ApiResponse<CalculateFitnessMetricsResponse>.Success(result.Value, "Calculated successfully.", StatusCodes.Status200OK);
                return Results.Ok(response);
            }
            
            var statusCode = result.Error?.Code == "FCE_STATS_NOT_FOUND" 
                ? StatusCodes.Status404NotFound 
                : StatusCodes.Status400BadRequest;

            var errorResponse = ApiResponse<CalculateFitnessMetricsResponse>.Failure(
                result.Errors.Select(e => e.Message), 
                result.Error?.Message ?? "Failed to process request", 
                statusCode);

            return statusCode == StatusCodes.Status404NotFound 
                ? Results.NotFound(errorResponse) 
                : Results.BadRequest(errorResponse);
        })
        .WithName("CalculateFitnessMetrics")
        .Produces<ApiResponse<CalculateFitnessMetricsResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);
    }
}
