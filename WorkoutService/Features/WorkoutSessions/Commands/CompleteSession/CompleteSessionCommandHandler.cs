using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkoutService.Common;
using WorkoutService.Data;
using WorkoutService.Data.Entities;

namespace WorkoutService.Features.WorkoutSessions.Commands.CompleteSession;

public class CompleteSessionCommandHandler : IRequestHandler<CompleteSessionCommand, Result>
{
	private readonly IGeneralRepo<WorkoutSession> _sessionRepo;
	private readonly IUnitOfWork _unitOfWork;

	public CompleteSessionCommandHandler(IGeneralRepo<WorkoutSession> sessionRepo, IUnitOfWork unitOfWork)
	{
		_sessionRepo = sessionRepo;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result> Handle(CompleteSessionCommand request, CancellationToken cancellationToken)
	{
		return await _unitOfWork.ExecuteAsync(async () =>
		{

			var rowsAffected = await _sessionRepo.GetAll()
				.Where(s => s.SessionId == request.SessionId && s.UserId == request.UserId && s.Status == "Active")
				.ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, "Completed"), cancellationToken);

			if (rowsAffected == 0)
				return Result.Failure(Error.NotFound("SESSION_NOT_FOUND", "Workout session not found, does not belong to the user, or is not active."));

			return Result.Success();
		}, cancellationToken);
	}
}
