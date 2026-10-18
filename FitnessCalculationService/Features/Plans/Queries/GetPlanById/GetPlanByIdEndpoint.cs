using FitnessCalculationService.Common;
using FitnessCalculationService.Common.Http;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCalculationService.Features.Plans.Queries.GetPlanById;

public static class GetPlanByIdEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/fitness/plans/{planId}", async (
            [FromRoute] string planId,
            IMediator mediator) =>
        {
            var query = new GetPlanByIdQuery(planId);
            var result = await mediator.Send(query);
            return result.ToHttpResult("Plan retrieved successfully.", StatusCodes.Status200OK);
        })
        .RequireAuthorization()
        .WithName("GetPlanById")
        .Produces<ApiResponse<PlanDetailResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);
    }
}
