using MediatR;
using WorkoutService.Common;

namespace WorkoutService.Features.Workouts.Queries.GetWorkoutById;

public record GetWorkoutByIdQuery(int Id) : IRequest<Result<GetWorkoutByIdResponse>>;
