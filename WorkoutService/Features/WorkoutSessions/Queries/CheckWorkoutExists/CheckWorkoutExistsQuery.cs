using MediatR;
using WorkoutService.Common;

namespace WorkoutService.Features.WorkoutSessions.Queries.CheckWorkoutExists;

public record CheckWorkoutExistsQuery(int WorkoutId) : IRequest<Result>;
