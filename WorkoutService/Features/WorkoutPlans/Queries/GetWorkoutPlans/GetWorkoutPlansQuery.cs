using MediatR;
using WorkoutService.Common;

namespace WorkoutService.Features.WorkoutPlans.Queries.GetWorkoutPlans;

public record GetWorkoutPlansQuery(
    int Page = 1,
    int PageSize = 10
) : IRequest<Result<IEnumerable<GetWorkoutPlansResponse>>>;

