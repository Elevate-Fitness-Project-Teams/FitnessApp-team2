using FitnessCalculationService.Common;
using FitnessCalculationService.Common.Http;
using FitnessCalculationService.Common.Paging;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCalculationService.Features.Plans.Queries.GetPlanConfigs;

public static class GetPlanConfigsEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/fitness/plan-configs", async (
            [FromQuery] string? goal,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            IMediator mediator = null!) =>
        {
            var query = new GetPlanConfigsQuery(goal, status, page, pageSize);
            var result = await mediator.Send(query);
            return result.ToHttpResult("Plan configurations retrieved successfully.", StatusCodes.Status200OK);
        })
        .RequireAuthorization()
        .WithName("GetPlanConfigs")
        .Produces<ApiResponse<PagedResult<PlanConfigListItem>>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);
    }
}
