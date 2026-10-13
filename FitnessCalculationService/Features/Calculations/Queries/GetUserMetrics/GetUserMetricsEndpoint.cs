using FitnessCalculationService.Common;
using FitnessCalculationService.Common.Http;
using FitnessCalculationService.Common.Security;
using MediatR;

namespace FitnessCalculationService.Features.Calculations.Queries.GetUserMetrics;

public static class GetUserMetricsEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/fitness/metrics/{userId}", async (string userId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetUserMetricsQuery(userId));
            return result.ToHttpResult();
        })
        .RequireAuthorization()
        .AddEndpointFilter(new OwnershipFilter("userId"))
        .WithName("GetUserMetrics")
        .Produces<ApiResponse<GetUserMetricsResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
        .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
        .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);
    }
}
