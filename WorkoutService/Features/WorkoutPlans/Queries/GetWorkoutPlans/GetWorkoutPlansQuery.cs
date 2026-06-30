using MediatR;
using WorkoutService.Common;

namespace WorkoutService.Features.WorkoutPlans.Queries.GetWorkoutPlans;

public record GetWorkoutPlansQuery : IRequest<Result<IEnumerable<GetWorkoutPlansResponse>>>;
