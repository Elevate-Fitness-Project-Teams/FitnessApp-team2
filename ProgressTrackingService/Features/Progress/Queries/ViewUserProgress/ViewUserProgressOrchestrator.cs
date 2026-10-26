using MediatR;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Errors;

using ProgressTrackingService.Features.Progress.Queries.GetWeightHistory;

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
        if (!userExistsResult.IsSuccess || !userExistsResult.Value)
        {
            return Result<ViewUserProgressResponse>.Failure(Error.NotFound(ProgressErrors.ResUserNotFound, "User not found"));
        }

        var userStatsResult = await _mediator.Send(new GetUserStatisticsQuery(request.UserId), cancellationToken);
        if (!userStatsResult.IsSuccess || userStatsResult.Value == null)
        {
            return Result<ViewUserProgressResponse>.Failure(userStatsResult.Error ?? Error.NotFound(ProgressErrors.ResUserNotFound, "User statistics not found."));
        }

        var workoutLogsResult = await _mediator.Send(new GetWorkoutLogsQuery(request.UserId), cancellationToken);
        var weightHistoryResult = await _mediator.Send(new GetWeightHistoryQuery(request.UserId, Page: 1, PageSize: 30), cancellationToken);

        var response = new ViewUserProgressResponse
        {
            Statistics = userStatsResult.Value,
            WorkoutLogs = (workoutLogsResult.IsSuccess && workoutLogsResult.Value != null)
                ? workoutLogsResult.Value
                : new List<WorkoutLogDto>(),
            WeightHistory = (weightHistoryResult.IsSuccess && weightHistoryResult.Value != null)
                ? weightHistoryResult.Value.Items.ToList()
                : new List<WeightHistoryDto>()
        };

        return Result<ViewUserProgressResponse>.Success(response);
    }
}
