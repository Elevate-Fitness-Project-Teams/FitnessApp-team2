using MediatR;
using WorkoutService.Common;

namespace WorkoutService.Features.WorkoutSessions.Orchestrators.StartSession;

public record StartSessionOrchestrator(Guid UserId, Guid WorkoutId) : IRequest<Result<StartSessionResponse>>;

