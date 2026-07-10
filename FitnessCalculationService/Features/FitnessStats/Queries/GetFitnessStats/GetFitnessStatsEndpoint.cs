using FitnessCalculationService.Common;
using MediatR;

namespace FitnessCalculationService.Features.FitnessStats.Queries.GetFitnessStats;

public static class GetFitnessStatsEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/fitness/stats/{userId}", async (string userId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetFitnessStatsQuery(userId));
            
            if (result.IsSuccess)
            {
                var response = ApiResponse<GetFitnessStatsResponse>.Success(result.Value);
                return Results.Ok(response);
            }
            
            var errorResponse = ApiResponse<GetFitnessStatsResponse>.Failure(result.Errors.Select(e => e.Message), result.Error.Message, StatusCodes.Status400BadRequest);
            return Results.BadRequest(errorResponse);
        })
        .WithName("GetFitnessStats")
        .Produces<ApiResponse<GetFitnessStatsResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
        .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);
    }
}
