using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkoutService.Common;
using WorkoutService.Data;
using WorkoutService.Data.Entities;

namespace WorkoutService.Features.WorkoutSessions.Queries.CheckActiveSession;

public class CheckActiveSessionQueryHandler : IRequestHandler<CheckActiveSessionQuery, Result>
{
	private readonly IGeneralRepo<WorkoutSession> _sessionRepo;
	public CheckActiveSessionQueryHandler(IGeneralRepo<WorkoutSession> sessionRepo)
	{
		_sessionRepo = sessionRepo;
	}

	public async Task<Result> Handle(CheckActiveSessionQuery request, CancellationToken cancellationToken)
	{
		var activeSession = await _sessionRepo.GetAll()
		.AnyAsync(s => s.UserId == request.UserId && s.Status == "Active", cancellationToken);
		if (activeSession)
			return Result.Failure(Error.Conflict(WorkoutErrorCodes.SessionAlreadyActive, "User already has an active workout session."));

		return Result.Success();
	}
}

