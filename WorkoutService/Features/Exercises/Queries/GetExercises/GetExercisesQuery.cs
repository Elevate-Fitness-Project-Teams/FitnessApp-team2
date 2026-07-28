using MediatR;
using WorkoutService.Common;

namespace WorkoutService.Features.Exercises.Queries.GetExercises;

public record GetExercisesQuery(
    int Page = 1,
    int PageSize = 10
) : IRequest<Result<IEnumerable<GetExercisesResponse>>>;

