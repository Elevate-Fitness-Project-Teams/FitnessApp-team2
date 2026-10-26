using Grpc.Core;
using MediatR;
using WorkoutService.Features.WorkoutSessions.Commands.CompleteSession;
using WorkoutService.Grpc;

namespace WorkoutService.GrpcServices;

public class WorkoutSessionGrpcServiceImpl : WorkoutSessionGrpcService.WorkoutSessionGrpcServiceBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<WorkoutSessionGrpcServiceImpl> _logger;

    public WorkoutSessionGrpcServiceImpl(IMediator mediator, ILogger<WorkoutSessionGrpcServiceImpl> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override async Task<CompleteSessionGrpcResponse> CompleteSession(
        CompleteSessionGrpcRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("gRPC CompleteSession called for SessionId: {SessionId}, UserId: {UserId}",
            request.SessionId, request.UserId);

        if (!Guid.TryParse(request.UserId, out var userId))
        {
            return new CompleteSessionGrpcResponse
            {
                IsSuccess = false,
                ErrorCode = "VAL_REQUIRED_FIELD",
                ErrorMessage = "Invalid UserId format."
            };
        }

        var command = new CompleteSessionCommand(request.SessionId, userId);
        var result = await _mediator.Send(command, context.CancellationToken);

        if (result.IsFailure)
        {
            var isNotFound = result.Errors.Any(e => e.Code == "SESSION_NOT_FOUND") || result.Error.Code == "SESSION_NOT_FOUND";
            return new CompleteSessionGrpcResponse
            {
                IsSuccess = false,
                ErrorCode = isNotFound ? "SESSION_NOT_FOUND" : result.Error.Code,
                ErrorMessage = result.Error.Description
            };
        }

        return new CompleteSessionGrpcResponse
        {
            IsSuccess = true,
            ErrorCode = string.Empty,
            ErrorMessage = "Workout session completed successfully."
        };
    }
}
