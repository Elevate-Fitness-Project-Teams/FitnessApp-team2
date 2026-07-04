using MediatR;
using WorkoutService.Common;

namespace WorkoutService.Features.WorkoutSessions.Commands.CheckWorkoutExists;

public record CheckWorkoutExistsCommand(int WorkoutId) : IRequest<Result>;
