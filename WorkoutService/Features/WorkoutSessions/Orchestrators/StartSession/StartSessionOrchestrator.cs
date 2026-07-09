using MediatR;
using WorkoutService.Common;

namespace WorkoutService.Features.WorkoutSessions.Orchestrators.StartSession;

public record StartSessionOrchestrator(int UserId, int WorkoutId) : IRequest<Result<StartSessionResponse>>;
