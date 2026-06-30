using MediatR;
using WorkoutService.Common;

namespace WorkoutService.Features.WorkoutSessions.Commands.StartSession;

public record StartSessionCommand(int UserId, int WorkoutId) : IRequest<Result<StartSessionResponse>>;
