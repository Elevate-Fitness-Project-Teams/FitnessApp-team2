using MediatR;
using WorkoutService.Common;

namespace WorkoutService.Features.Workouts.Queries.GetWorkouts;

public record GetWorkoutsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Category = null,
    string? Difficulty = null,
    int? Duration = null,
    string? Search = null
) : IRequest<Result<IEnumerable<GetWorkoutsResponse>>>;
