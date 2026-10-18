using FitnessCalculationService.Common;
using FitnessCalculationService.Common.Http;
using FitnessCalculationService.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCalculationService.Features.Plans.Queries.GetActivePlan;

public static class GetActivePlanEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/fitness/plans/active/{userId:guid}", async (
            [FromRoute] Guid userId,
            ICurrentUser currentUser,
            IMediator mediator) =>
        {
            var query = new GetActivePlanQuery(userId);
            var result = await mediator.Send(query);
            return result.ToHttpResult("Active plan retrieved successfully.", StatusCodes.Status200OK);
        })
        .RequireAuthorization()
        .AddEndpointFilter(new OwnershipFilter("userId"))
        .WithName("GetActivePlan")
        .WithTags("Plans")
        .WithSummary("Get currently active assigned plan for a user")
        .WithDescription("Retrieves the currently assigned workout plan configuration for the specified user.")
        .Produces<ApiResponse<ActivePlanDetailResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
        .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);
    }
}
