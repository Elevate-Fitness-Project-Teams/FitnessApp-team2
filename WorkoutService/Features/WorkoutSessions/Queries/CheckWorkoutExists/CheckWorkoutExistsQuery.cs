using MediatR;
using WorkoutService.Common;

namespace WorkoutService.Features.WorkoutSessions.Queries.CheckWorkoutExists;

public record CheckWorkoutExistsQuery(Guid WorkoutId) : IRequest<Result>;

