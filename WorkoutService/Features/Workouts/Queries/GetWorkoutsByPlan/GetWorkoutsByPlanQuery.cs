using MediatR;
using WorkoutService.Common;
using WorkoutService.Features.Workouts.Queries.GetWorkouts;

namespace WorkoutService.Features.Workouts.Queries.GetWorkoutsByPlan;

public record GetWorkoutsByPlanQuery(int PlanId) : IRequest<Result<IEnumerable<GetWorkoutsResponse>>>;
