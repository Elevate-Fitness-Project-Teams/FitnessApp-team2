using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ProgressTrackingService.Common;
using System.Security.Claims;
using ProgressTrackingService.Common.Extensions;
using ProgressTrackingService.Common.Security;
using ProgressTrackingService.Features.WorkoutLogs.DTOs;

namespace ProgressTrackingService.Features.WorkoutLogs.Orchestrators.LogWorkout;

public static class LogWorkoutEndpoint
{
    public static void MapLogWorkoutEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/progress/workouts", async (
            [FromBody] LogWorkoutRequest request,
            HttpContext httpContext,
            IMediator mediator) =>
        {
            var userId = httpContext.GetUserId();
            if (!userId.HasValue)
            {
                return Results.Unauthorized();
            }

            var orchestrator = new LogWorkoutOrchestrator(
                userId.Value,
                request.WorkoutId,
                request.SessionId,
                request.CompletedAt,
                request.Duration,
                request.CaloriesBurned,
                request.Difficulty,
                request.Notes,
                request.Rating,
                request.ExercisesCompleted?.Select(e => new LogWorkoutExerciseDto
                {
                    ExerciseId = e.ExerciseId,
                    Sets = e.Sets,
                    Reps = e.Reps,
                    WeightUsed = e.WeightUsed,
                    Completed = e.Completed
                }).ToList() ?? new List<LogWorkoutExerciseDto>()
            );

            var result = await mediator.Send(orchestrator);

            if (!result.IsSuccess)
            {
                return result.ToHttpResult();
            }

            return Results.Created(
                $"/api/v1/progress/workouts/{result.Value.LogId}",
                ApiResponse<LogWorkoutResponse>.Success(result.Value));
        })
        .RequireAuthorization()
        .WithName("LogWorkout")
        .WithTags("ProgressTracking");
    }
}
