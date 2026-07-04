using MediatR;
using WorkoutService.Common;
using WorkoutService.Features.WorkoutSessions.Orchestrators.StartSession;

namespace WorkoutService.Features.WorkoutSessions.Commands.CreateSession;

public record CreateSessionCommand(int UserId, int WorkoutId) : IRequest<Result<StartSessionResponse>>;
