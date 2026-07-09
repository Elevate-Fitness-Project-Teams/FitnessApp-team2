using MediatR;
using Microsoft.AspNetCore.Mvc;
using WorkoutService.Common;
using WorkoutService.Features.Workouts.Queries.GetWorkoutById;
using WorkoutService.Features.Workouts.Queries.GetWorkouts;
using WorkoutService.Features.Workouts.Queries.GetWorkoutsByCategory;
using WorkoutService.Features.Workouts.Queries.GetWorkoutsByPlan;

namespace WorkoutService.Controllers;

[ApiController]
[Route("api/v1/workouts")]
public class WorkoutsController : ControllerBase
{
    private readonly IMediator _mediator;

    public WorkoutsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetWorkouts([FromQuery] GetWorkoutsQuery query)
    {
        var result = await _mediator.Send(query);
        
        if (result.IsFailure)
            return StatusCode(400, ApiResponse<GetWorkoutsResponse>.Failure(result.Errors.Select(e => e.Description), result.Error.Description));

        return Ok(ApiResponse<IEnumerable<GetWorkoutsResponse>>.Success(result.Value, "Workouts fetched successfully."));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetWorkoutById(int id)
    {
        var result = await _mediator.Send(new GetWorkoutByIdQuery(id));

        if (result.IsFailure)
        {
            var isNotFound = result.Errors.Any(e => e.Code == WorkoutErrorCodes.WorkoutNotFound);
            var statusCode = isNotFound ? 404 : 400;

            return StatusCode(statusCode, ApiResponse<GetWorkoutByIdResponse>.Failure(
                result.Errors.Select(e => e.Description),
                result.Error.Description,
                statusCode));
        }

        return Ok(ApiResponse<GetWorkoutByIdResponse>.Success(result.Value, "Workout resolved."));
    }

    [HttpGet("by-plan/{planId}")]
    public async Task<IActionResult> GetWorkoutsByPlan(int planId)
    {
        var result = await _mediator.Send(new GetWorkoutsByPlanQuery(planId));

        if (result.IsFailure)
            return StatusCode(400, ApiResponse<GetWorkoutsResponse>.Failure(result.Errors.Select(e => e.Description), result.Error.Description));

        return Ok(ApiResponse<IEnumerable<GetWorkoutsResponse>>.Success(result.Value, "Workouts fetched successfully."));
    }

    [HttpGet("category/{categoryName}")]
    public async Task<IActionResult> GetWorkoutsByCategory(string categoryName)
    {
        var result = await _mediator.Send(new GetWorkoutsByCategoryQuery(categoryName));

        if (result.IsFailure)
            return StatusCode(400, ApiResponse<GetWorkoutsResponse>.Failure(result.Errors.Select(e => e.Description), result.Error.Description));

        return Ok(ApiResponse<IEnumerable<GetWorkoutsResponse>>.Success(result.Value, "Workouts fetched successfully."));
    }
}
