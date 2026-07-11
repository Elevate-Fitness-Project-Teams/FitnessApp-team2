using MediatR;
using Microsoft.AspNetCore.Mvc;
using WorkoutService.Common;
using WorkoutService.Features.WorkoutPlans.Queries.GetWorkoutPlanById;
using WorkoutService.Features.WorkoutPlans.Queries.GetWorkoutPlans;
using Microsoft.AspNetCore.Authorization;

namespace WorkoutService.Controllers;

[ApiController]
[Route("api/v1/workout-plans")]
[Authorize]
public class WorkoutPlansController : ControllerBase
{
    private readonly IMediator _mediator;

    public WorkoutPlansController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetWorkoutPlans()
    {
        var result = await _mediator.Send(new GetWorkoutPlansQuery());
        
        if (result.IsFailure)
            return StatusCode(400, ApiResponse<GetWorkoutPlansResponse>.Failure(result.Errors.Select(e => e.Description), result.Error.Description));

        return Ok(ApiResponse<IEnumerable<GetWorkoutPlansResponse>>.Success(result.Value, "Workout plans fetched successfully."));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetWorkoutPlanById(Guid id)
    {
        var result = await _mediator.Send(new GetWorkoutPlanByIdQuery(id.ToString()));

        if (result.IsFailure)
        {
            var isNotFound = result.Errors.Any(e => e.Code == WorkoutErrorCodes.WorkoutPlanNotFound);
            var statusCode = isNotFound ? 404 : 400;

            return StatusCode(statusCode, ApiResponse<GetWorkoutPlanByIdResponse>.Failure(
                result.Errors.Select(e => e.Description),
                result.Error.Description,
                statusCode));
        }

        return Ok(ApiResponse<GetWorkoutPlanByIdResponse>.Success(result.Value, "Workout plan resolved."));
    }
}
