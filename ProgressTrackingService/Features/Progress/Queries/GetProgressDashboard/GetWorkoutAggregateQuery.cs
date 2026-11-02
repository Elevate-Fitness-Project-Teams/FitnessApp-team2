using MediatR;
using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.Progress.Queries.GetProgressDashboard;

public record WorkoutAggregateDto(int WorkoutsCount, int TotalCalories, int TotalMinutes, double AvgRating);

public record GetWorkoutAggregateQuery(Guid UserId, DateTime From, DateTime To) : IRequest<Result<WorkoutAggregateDto>>;

public class GetWorkoutAggregateQueryHandler : IRequestHandler<GetWorkoutAggregateQuery, Result<WorkoutAggregateDto>>
{
    private readonly IGeneralRepo<WorkoutLog> _workoutRepo;

    public GetWorkoutAggregateQueryHandler(IGeneralRepo<WorkoutLog> workoutRepo)
    {
        _workoutRepo = workoutRepo;
    }

    public async Task<Result<WorkoutAggregateDto>> Handle(GetWorkoutAggregateQuery request, CancellationToken cancellationToken)
    {
        var from = request.From.Date;
        var to = request.To.Date.AddDays(1).AddTicks(-1);

        var query = _workoutRepo.Find(w => w.UserId == request.UserId && w.CompletedAt >= from && w.CompletedAt <= to)
            .AsNoTracking();

        var count = await query.CountAsync(cancellationToken);
        if (count == 0)
        {
            return Result.Success(new WorkoutAggregateDto(0, 0, 0, 0.0));
        }

        var totalCalories = await query.SumAsync(w => w.CaloriesBurned, cancellationToken);
        var totalMinutes = await query.SumAsync(w => w.DurationInMinutes, cancellationToken);
        var avgRating = await query.AverageAsync(w => (double)w.Rating, cancellationToken);

        return Result.Success(new WorkoutAggregateDto(count, totalCalories, totalMinutes, Math.Round(avgRating, 1)));
    }
}
