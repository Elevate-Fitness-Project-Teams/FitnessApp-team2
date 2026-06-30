using MediatR;
using Microsoft.AspNetCore.Mvc;
using WorkoutService.Common;
using WorkoutService.Features.Exercises.Queries.GetExerciseById;
using WorkoutService.Features.Exercises.Queries.GetExercises;

namespace WorkoutService.Controllers;

[ApiController]
[Route("api/v1/exercises")]
public class ExercisesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExercisesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetExercises()
    {
        var result = await _mediator.Send(new GetExercisesQuery());
        
        if (result.IsFailure)
            return StatusCode(400, ApiResponse<object>.Failure(result.Errors.Select(e => e.Description), result.Error.Description));

        return Ok(ApiResponse<IEnumerable<GetExercisesResponse>>.Success(result.Value, "Exercises fetched successfully."));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetExerciseById(int id)
    {
        var result = await _mediator.Send(new GetExerciseByIdQuery(id));

        if (result.IsFailure)
        {
            var isNotFound = result.Errors.Any(e => e.Code == WorkoutErrorCodes.ExerciseNotFound);
            var statusCode = isNotFound ? 404 : 400;

            return StatusCode(statusCode, ApiResponse<object>.Failure(
                result.Errors.Select(e => e.Description),
                result.Error.Description,
                statusCode));
        }

        return Ok(ApiResponse<GetExerciseByIdResponse>.Success(result.Value, "Exercise resolved."));
    }
}
