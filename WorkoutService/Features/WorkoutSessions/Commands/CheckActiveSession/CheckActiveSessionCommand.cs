using MediatR;
using WorkoutService.Common;

namespace WorkoutService.Features.WorkoutSessions.Commands.CheckActiveSession;

public record CheckActiveSessionCommand(int UserId) : IRequest<Result>;
