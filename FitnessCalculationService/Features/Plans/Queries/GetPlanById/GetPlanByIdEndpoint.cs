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
        .WithTags("Plans")
        .WithSummary("Get plan configuration by plan identifier")
        .WithDescription("Retrieves detailed workout plan template by its business identifier (e.g. LW-N, MW-H). Backed by Redis distributed caching with a 24-hour TTL.")
        .Produces<ApiResponse<PlanDetailResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);
    }
}
