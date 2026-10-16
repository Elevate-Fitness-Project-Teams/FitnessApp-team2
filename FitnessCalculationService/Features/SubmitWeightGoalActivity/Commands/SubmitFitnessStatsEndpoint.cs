using FitnessCalculationService.Common;
using FitnessCalculationService.Common.Http;
using FitnessCalculationService.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCalculationService.Features.SubmitWeightGoalActivity.Commands;

public static class SubmitFitnessStatsEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var handler = async ([FromBody] SubmitFitnessStatsCommand command, ICurrentUser currentUser, IMediator mediator) =>
        {
            var effectiveUserId = command.UserId == Guid.Empty && currentUser.UserId.HasValue
                ? currentUser.UserId.Value
                : command.UserId;

            var effectiveCommand = command with { UserId = effectiveUserId };

            var result = await mediator.Send(effectiveCommand);
            if (!result.IsSuccess)
            {
                return result.ToHttpResult("Failed to record fitness stats.", StatusCodes.Status400BadRequest);
            }

            var response = new SubmitFitnessStatsResponse(result.Value, DateTime.UtcNow);
            return Result<SubmitFitnessStatsResponse>.Success(response)
                .ToHttpResult("Fitness stats recorded successfully.", StatusCodes.Status201Created, $"/api/v1/fitness/stats/{effectiveUserId}");
        };

        app.MapPost("/api/v1/fitness/stats", handler)
            .RequireAuthorization()
            .AddEndpointFilter(new OwnershipFilter("userId"))
            .WithName("SubmitFitnessStats")
            .Produces<ApiResponse<SubmitFitnessStatsResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden);

        app.MapPost("/api/v1/fitness/weight-goal-activity", handler)
            .RequireAuthorization()
            .AddEndpointFilter(new OwnershipFilter("userId"))
            .WithName("SubmitWeightGoalActivity")
            .Produces<ApiResponse<SubmitFitnessStatsResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden);
    }
}
