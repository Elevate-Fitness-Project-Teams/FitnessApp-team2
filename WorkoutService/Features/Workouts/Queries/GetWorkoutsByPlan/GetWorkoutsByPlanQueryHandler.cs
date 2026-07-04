using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkoutService.Common;
using WorkoutService.Data;
using WorkoutService.Data.Entities;
using WorkoutService.Features.Workouts.Queries.GetWorkouts;

namespace WorkoutService.Features.Workouts.Queries.GetWorkoutsByPlan;

public class GetWorkoutsByPlanQueryHandler : IRequestHandler<GetWorkoutsByPlanQuery, Result<IEnumerable<GetWorkoutsResponse>>>
{
    private readonly IGeneralRepo<Workout> _workoutRepo;

    public GetWorkoutsByPlanQueryHandler(IGeneralRepo<Workout> workoutRepo)
    {
        _workoutRepo = workoutRepo;
    }

    public async Task<Result<IEnumerable<GetWorkoutsResponse>>> Handle(GetWorkoutsByPlanQuery request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var skip = (Math.Max(request.Page, 1) - 1) * pageSize;

        var workouts = await _workoutRepo.GetAll()
            .AsNoTracking()
            .Where(w => w.WorkoutPlanId == request.PlanId)
            .OrderBy(w => w.Id)
            .Skip(skip)
            .Take(pageSize)
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
