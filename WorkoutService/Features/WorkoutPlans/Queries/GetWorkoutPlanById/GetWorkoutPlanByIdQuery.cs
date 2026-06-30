using MediatR;
using WorkoutService.Common;

namespace WorkoutService.Features.WorkoutPlans.Queries.GetWorkoutPlanById;

public record GetWorkoutPlanByIdQuery(string PlanId) : IRequest<Result<GetWorkoutPlanByIdResponse>>;
