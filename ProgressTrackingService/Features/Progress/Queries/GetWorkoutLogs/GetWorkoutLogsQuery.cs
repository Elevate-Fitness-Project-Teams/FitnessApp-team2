using MediatR;
using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.Progress.Queries.ViewUserProgress;

public record GetWorkoutLogsQuery(Guid UserId, int Limit = 10) : IRequest<Result<List<WorkoutLogDto>>>;

public class GetWorkoutLogsQueryHandler : IRequestHandler<GetWorkoutLogsQuery, Result<List<WorkoutLogDto>>>
{
    private readonly IGeneralRepo<WorkoutLog> _workoutLogRepo;

    public GetWorkoutLogsQueryHandler(IGeneralRepo<WorkoutLog> workoutLogRepo)
    {
        _workoutLogRepo = workoutLogRepo;
    }

    public async Task<Result<List<WorkoutLogDto>>> Handle(GetWorkoutLogsQuery request, CancellationToken cancellationToken)
    {
        var limit = request.Limit <= 0 ? 10 : (request.Limit > 100 ? 100 : request.Limit);

        var result = await _workoutLogRepo.Find(w => w.UserId == request.UserId)
            .AsNoTracking()
            .OrderByDescending(w => w.CompletedAt)
            .Take(limit)
            .Select(wl => new WorkoutLogDto
            {
                Id = wl.Id,
                WorkoutId = wl.WorkoutId,
                SessionId = wl.SessionId,
                DurationInMinutes = wl.DurationInMinutes,
                CaloriesBurned = wl.CaloriesBurned,
                Difficulty = wl.Difficulty.ToString(),
                Rating = wl.Rating,
                Notes = wl.Notes,
                CompletedAt = wl.CompletedAt
            })
            .ToListAsync(cancellationToken);
            
        return Result<List<WorkoutLogDto>>.Success(result);
    }
}
