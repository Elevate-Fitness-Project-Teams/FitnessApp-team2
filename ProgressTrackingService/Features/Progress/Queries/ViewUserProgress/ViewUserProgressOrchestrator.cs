using MediatR;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Errors;
using ProgressTrackingService.Features.Progress.Queries.GetProgressStats;
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

        var progressStatsResult = await _mediator.Send(new GetProgressStatsQuery(request.UserId), cancellationToken);
        if (!progressStatsResult.IsSuccess)
        {
            return Result<ViewUserProgressResponse>.Failure(progressStatsResult.Error);
        }

        var workoutLogsResult = await _mediator.Send(new GetWorkoutLogsQuery(request.UserId, Limit: 10), cancellationToken);
        var weightHistoryResult = await _mediator.Send(new GetWeightHistoryQuery(request.UserId, Page: 1, PageSize: 30), cancellationToken);

        var response = new ViewUserProgressResponse
        {
            Statistics = progressStatsResult.Value,
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
