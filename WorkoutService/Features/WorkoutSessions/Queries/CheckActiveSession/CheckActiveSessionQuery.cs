using MediatR;
using WorkoutService.Common;

namespace WorkoutService.Features.WorkoutSessions.Queries.CheckActiveSession;

public record CheckActiveSessionQuery(int UserId) : IRequest<Result>;
