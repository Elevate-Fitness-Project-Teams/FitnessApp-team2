using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkoutService.Common;
using WorkoutService.Data;
using WorkoutService.Data.Entities;
using WorkoutService.Features.Workouts.Queries.GetWorkouts;

namespace WorkoutService.Features.Workouts.Queries.GetWorkoutsByCategory;

public class GetWorkoutsByCategoryQueryHandler : IRequestHandler<GetWorkoutsByCategoryQuery, Result<IEnumerable<GetWorkoutsResponse>>>
{
    private readonly IGeneralRepo<Workout> _workoutRepo;

    public GetWorkoutsByCategoryQueryHandler(IGeneralRepo<Workout> workoutRepo)
    {
        _workoutRepo = workoutRepo;
    }

    public async Task<Result<IEnumerable<GetWorkoutsResponse>>> Handle(GetWorkoutsByCategoryQuery request, CancellationToken cancellationToken)
    {
        var workouts = await _workoutRepo.GetAll()
            .AsNoTracking()
            .Where(w => w.Category == request.CategoryName)
            .OrderBy(w => w.Id)
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
