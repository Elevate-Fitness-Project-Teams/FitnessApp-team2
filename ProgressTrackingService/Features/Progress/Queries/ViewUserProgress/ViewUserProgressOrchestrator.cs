using MediatR;
using ProgressTrackingService.Common;

namespace ProgressTrackingService.Features.Progress.Queries.ViewUserProgress;

public record ViewUserProgressOrchestrator(Guid UserId) : IRequest<Result<ViewUserProgressResponse>>;

public class ViewUserProgressOrchestratorHandler : IRequestHandler<ViewUserProgressOrchestrator, Result<ViewUserProgressResponse>>
{
    private readonly IMediator _mediator;

    public ViewUserProgressOrchestratorHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Result<ViewUserProgressResponse>> Handle(ViewUserProgressOrchestrator request, CancellationToken cancellationToken)
    {
        var userExistsResult = await _mediator.Send(new CheckUserExistsQuery(request.UserId), cancellationToken);
        if (!userExistsResult.Value)
        {
            return Result<ViewUserProgressResponse>.Failure(Error.NotFound("RES_USER_NOT_FOUND", "User not found"));
        }

        var userStatsResult = await _mediator.Send(new GetUserStatisticsQuery(request.UserId), cancellationToken);
        var workoutLogsResult = await _mediator.Send(new GetWorkoutLogsQuery(request.UserId), cancellationToken);
        var weightHistoryResult = await _mediator.Send(new GetWeightHistoryQuery(request.UserId), cancellationToken);

        var response = new ViewUserProgressResponse
        {
            Statistics = userStatsResult.Value ?? new UserStatisticDto(),
            WorkoutLogs = workoutLogsResult.Value,
            WeightHistory = weightHistoryResult.Value
        };

        return Result<ViewUserProgressResponse>.Success(response);
    }
}
