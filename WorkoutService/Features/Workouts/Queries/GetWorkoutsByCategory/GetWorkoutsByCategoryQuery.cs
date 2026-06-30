using MediatR;
using WorkoutService.Common;
using WorkoutService.Features.Workouts.Queries.GetWorkouts;

namespace WorkoutService.Features.Workouts.Queries.GetWorkoutsByCategory;

public record GetWorkoutsByCategoryQuery(string CategoryName) : IRequest<Result<IEnumerable<GetWorkoutsResponse>>>;
