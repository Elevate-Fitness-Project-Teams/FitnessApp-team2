using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkoutService.Common;
using WorkoutService.Data;
using WorkoutService.Data.Entities;

namespace WorkoutService.Features.Workouts.Queries.GetWorkouts;

public class GetWorkoutsQueryHandler : IRequestHandler<GetWorkoutsQuery, Result<IEnumerable<GetWorkoutsResponse>>>
{
    private readonly IGeneralRepo<Workout> _workoutRepo;

    public GetWorkoutsQueryHandler(IGeneralRepo<Workout> workoutRepo)
    {
        _workoutRepo = workoutRepo;
    }

    public async Task<Result<IEnumerable<GetWorkoutsResponse>>> Handle(GetWorkoutsQuery request, CancellationToken cancellationToken)
    {
        var query = _workoutRepo.GetAll().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(w => w.Category == request.Category);
        }

        if (!string.IsNullOrWhiteSpace(request.Difficulty))
        {
            query = query.Where(w => w.Difficulty == request.Difficulty);
        }

        if (request.Duration.HasValue)
        {
            query = query.Where(w => w.DurationInMinutes == request.Duration.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(w => w.Name.Contains(request.Search));
        }

        var skip = (request.Page - 1) * request.PageSize;

        var workouts = await query
            .OrderBy(w => w.Id)
            .Skip(skip)
            .Take(request.PageSize)
            .Select(w => new GetWorkoutsResponse(
                w.Id,
                w.Name,
                w.DurationInMinutes,
                w.Difficulty
            ))
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<GetWorkoutsResponse>>.Success(workouts);
    }
}
