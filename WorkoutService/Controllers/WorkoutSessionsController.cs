using MediatR;
using Microsoft.AspNetCore.Mvc;
using WorkoutService.Common;
using WorkoutService.Features.WorkoutSessions.Commands.CompleteSession;
using WorkoutService.Features.WorkoutSessions.Orchestrators.StartSession;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace WorkoutService.Controllers;

public record StartSessionRequest(Guid WorkoutId);
public record CompleteSessionRequest(string SessionId);

[ApiController]
[Route("api/v1/sessions")]
[Authorize]
public class WorkoutSessionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public WorkoutSessionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartSession([FromBody] StartSessionRequest body)
    {
        var userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();

        var command = new StartSessionOrchestrator(userId, body.WorkoutId);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            var isNotFound = result.Errors.Any(e => e.Code == WorkoutErrorCodes.WorkoutNotFound);
            var statusCode = isNotFound ? 404 : 400;

            return StatusCode(statusCode, ApiResponse<StartSessionResponse>.Failure(
                result.Errors.Select(e => e.Description),
                result.Error.Description,
                statusCode));
        }

        return Ok(ApiResponse<StartSessionResponse>.Success(result.Value, "Workout session started successfully."));
    }

    [HttpPost("complete")]
    public async Task<IActionResult> CompleteSession([FromBody] CompleteSessionRequest body)
    {
        var userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();

        var command = new CompleteSessionCommand(body.SessionId, userId);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            var isNotFound = result.Errors.Any(e => e.Code == "SESSION_NOT_FOUND");
            var statusCode = isNotFound ? 404 : 400;

            return StatusCode(statusCode, ApiResponse<CompleteSessionCommand>.Failure(
                result.Errors.Select(e => e.Description),
                result.Error.Description,
                statusCode));
        }

        return Ok(ApiResponse<CompleteSessionCommand>.Success(command, "Workout session completed successfully."));
    }

}
