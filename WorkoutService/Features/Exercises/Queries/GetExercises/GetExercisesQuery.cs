using MediatR;
using WorkoutService.Common;

namespace WorkoutService.Features.Exercises.Queries.GetExercises;

public record GetExercisesQuery : IRequest<Result<IEnumerable<GetExercisesResponse>>>;
