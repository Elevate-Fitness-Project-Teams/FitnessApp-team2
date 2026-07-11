using MediatR;
using WorkoutService.Common;

namespace WorkoutService.Features.WorkoutSessions.Commands.CompleteSession;

public record CompleteSessionCommand(string SessionId, Guid UserId) : IRequest<Result>;

