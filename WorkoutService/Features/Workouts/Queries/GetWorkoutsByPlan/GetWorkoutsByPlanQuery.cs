using MediatR;
using WorkoutService.Common;
using WorkoutService.Features.Workouts.Queries.GetWorkouts;

namespace WorkoutService.Features.Workouts.Queries.GetWorkoutsByPlan;

public record GetWorkoutsByPlanQuery(
    int PlanId,
    int Page = 1,
    int PageSize = 10
) : IRequest<Result<IEnumerable<GetWorkoutsResponse>>>;
