using FitnessCalculationService.Common;
using FitnessCalculationService.Common.Http;
using FitnessCalculationService.Common.Paging;
using FitnessCalculationService.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCalculationService.Features.Plans.Queries.GetPlanHistory;

public static class GetPlanHistoryEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var handler = async (
            [FromRoute] Guid userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            ICurrentUser currentUser = null!,
            IMediator mediator = null!) =>
        {
            var query = new GetPlanHistoryQuery(userId, page, pageSize);
            var result = await mediator.Send(query);
            return result.ToHttpResult("Plan history retrieved successfully.", StatusCodes.Status200OK);
        };

        app.MapGet("/api/v1/fitness/plan-history/{userId:guid}", handler)
            .RequireAuthorization()
            .AddEndpointFilter(new OwnershipFilter("userId"))
            .WithName("GetPlanHistory")
            .Produces<ApiResponse<PagedResult<PlanHistoryItemResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden);

        app.MapGet("/api/v1/fitness/plans/history/{userId:guid}", handler)
            .RequireAuthorization()
            .AddEndpointFilter(new OwnershipFilter("userId"))
            .WithName("GetPlansHistory")
            .Produces<ApiResponse<PagedResult<PlanHistoryItemResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden);
    }
}
