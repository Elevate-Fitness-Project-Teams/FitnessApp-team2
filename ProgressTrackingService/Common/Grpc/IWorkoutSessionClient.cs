using ProgressTrackingService.Common;

namespace ProgressTrackingService.Common.Grpc;

public interface IWorkoutSessionClient
{
    Task<Result> CompleteSessionAsync(string sessionId, Guid userId, CancellationToken cancellationToken = default);
}
