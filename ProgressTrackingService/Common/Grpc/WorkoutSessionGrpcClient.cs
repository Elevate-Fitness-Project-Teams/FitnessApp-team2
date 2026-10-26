using Grpc.Core;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Errors;
using ProgressTrackingService.Grpc;

namespace ProgressTrackingService.Common.Grpc;

public class WorkoutSessionGrpcClient : IWorkoutSessionClient
{
    private readonly WorkoutSessionGrpcService.WorkoutSessionGrpcServiceClient _client;
    private readonly ILogger<WorkoutSessionGrpcClient> _logger;

    public WorkoutSessionGrpcClient(
        WorkoutSessionGrpcService.WorkoutSessionGrpcServiceClient client,
        ILogger<WorkoutSessionGrpcClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result> CompleteSessionAsync(string sessionId, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new CompleteSessionGrpcRequest
            {
                SessionId = sessionId,
                UserId = userId.ToString()
            };

            var response = await _client.CompleteSessionAsync(request, cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return Result.Success();
            }

            if (response.ErrorCode == "SESSION_NOT_FOUND")
            {
                return Result.Failure(Error.NotFound(ProgressErrors.ResSessionNotFound, response.ErrorMessage ?? "Workout session not found."));
            }

            _logger.LogWarning("WorkoutService gRPC returned failure for session {SessionId}: {Code} - {Message}",
                sessionId, response.ErrorCode, response.ErrorMessage);

            return Result.Failure(Error.NotFound(ProgressErrors.ResSessionNotFound, response.ErrorMessage ?? "Workout session not found."));
        }
        catch (RpcException ex) when (ex.StatusCode is StatusCode.NotFound or StatusCode.PermissionDenied or StatusCode.Unauthenticated)
        {
            _logger.LogWarning(ex, "gRPC session {SessionId} not found or permission denied.", sessionId);
            return Result.Failure(Error.NotFound(ProgressErrors.ResSessionNotFound, "Workout session not found or access denied."));
        }
        catch (RpcException ex) when (ex.StatusCode is StatusCode.Unavailable or StatusCode.DeadlineExceeded)
        {
            _logger.LogError(ex, "gRPC WorkoutService unavailable when completing session {SessionId}.", sessionId);
            return Result.Failure(Error.Unavailable(ProgressErrors.SrvServiceUnavailable, "Workout service is unavailable."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected failure in WorkoutSessionGrpcClient for session {SessionId}.", sessionId);
            return Result.Failure(Error.Unavailable(ProgressErrors.SrvServiceUnavailable, "Workout service is unavailable."));
        }
    }
}
