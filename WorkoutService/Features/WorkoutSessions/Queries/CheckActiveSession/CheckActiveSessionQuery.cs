using MediatR;
using WorkoutService.Common;

namespace WorkoutService.Features.WorkoutSessions.Queries.CheckActiveSession;

public record CheckActiveSessionQuery(Guid UserId) : IRequest<Result>;

