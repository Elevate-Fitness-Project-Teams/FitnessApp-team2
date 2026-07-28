using MediatR;
using WorkoutService.Common;

namespace WorkoutService.Features.Workouts.Queries.GetWorkoutById;

public record GetWorkoutByIdQuery(Guid Id) : IRequest<Result<GetWorkoutByIdResponse>>;

