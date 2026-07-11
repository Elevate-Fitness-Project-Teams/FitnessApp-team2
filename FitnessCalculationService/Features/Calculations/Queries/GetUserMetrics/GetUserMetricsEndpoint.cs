using FitnessCalculationService.Common;
using MediatR;

namespace FitnessCalculationService.Features.Calculations.Queries.GetUserMetrics;

public static class GetUserMetricsEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/fitness/metrics/{userId}", async (string userId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetUserMetricsQuery(userId));
            
            if (result.IsSuccess)
            {
                var response = ApiResponse<GetUserMetricsResponse>.Success(result.Value);
                return Results.Ok(response);
            }
            
            var errorResponse = ApiResponse<GetUserMetricsResponse>.Failure(result.Errors.Select(e => e.Message), result.Error.Message, StatusCodes.Status400BadRequest);
            return Results.BadRequest(errorResponse);
        })
        .WithName("GetUserMetrics")
        .RequireAuthorization()
        .Produces<ApiResponse<GetUserMetricsResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
        .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);
    }
}
