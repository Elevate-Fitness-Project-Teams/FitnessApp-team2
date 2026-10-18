using FitnessCalculationService.Common;
using FitnessCalculationService.Common.Http;
using FitnessCalculationService.Common.Security;
using FitnessCalculationService.Features.Plans.DTOs;
using FitnessCalculationService.Features.Plans.Orchestrators;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCalculationService.Features.Plans;

public static class AssignPlanEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/fitness/assign-plan", async (
            [FromBody] AssignPlanRequest request, 
            ICurrentUser currentUser, 
            IMediator mediator) =>
        {
            var effectiveUserId = request.UserId.HasValue && request.UserId.Value != Guid.Empty
                ? request.UserId.Value
                : (currentUser.UserId ?? Guid.Empty);

            var orchestrator = new AssignPlanOrchestrator(effectiveUserId, request.Reason, "user");
            var result = await mediator.Send(orchestrator);

            return result.ToHttpResult("Fitness plan assigned successfully.", StatusCodes.Status200OK);
        })
        .RequireAuthorization()
        .AddEndpointFilter(new OwnershipFilter("userId"))
        .WithName("AssignPlan")
        .WithTags("Plans")
        .WithSummary("Assign active workout plan to user")
        .WithDescription("Evaluates user fitness goal and caloric status tier, deactivates any existing active plan, assigns the matching plan configuration, records plan history, and emits domain events.")
        .Produces<ApiResponse<AssignPlanResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
        .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
        .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);
    }
}

