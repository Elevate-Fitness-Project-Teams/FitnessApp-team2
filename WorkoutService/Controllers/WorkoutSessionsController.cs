using MediatR;
using Microsoft.AspNetCore.Mvc;
using WorkoutService.Common;
using WorkoutService.Features.WorkoutSessions.Commands.CompleteSession;
using WorkoutService.Features.WorkoutSessions.Commands.StartSession;

namespace WorkoutService.Controllers;

[ApiController]
[Route("api/v1/sessions")]
public class WorkoutSessionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public WorkoutSessionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartSession([FromBody] StartSessionCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            var isNotFound = result.Errors.Any(e => e.Code == WorkoutErrorCodes.WorkoutNotFound);
            var statusCode = isNotFound ? 404 : 400;

            return StatusCode(statusCode, ApiResponse<object>.Failure(
                result.Errors.Select(e => e.Description),
                result.Error.Description,
                statusCode));
        }

        return Ok(ApiResponse<StartSessionResponse>.Success(result.Value, "Workout session started successfully."));
    }

    [HttpPost("complete")]
    public async Task<IActionResult> CompleteSession([FromBody] CompleteSessionCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            var isNotFound = result.Errors.Any(e => e.Code == "SESSION_NOT_FOUND");
            var statusCode = isNotFound ? 404 : 400;

            return StatusCode(statusCode, ApiResponse<object>.Failure(
                result.Errors.Select(e => e.Description),
                result.Error.Description,
                statusCode));
        }

        return Ok(ApiResponse<object>.Success(new object(), "Workout session completed successfully."));
    }

}
