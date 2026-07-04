using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkoutService.Common;
using WorkoutService.Data;
using WorkoutService.Data.Entities;

namespace WorkoutService.Features.WorkoutSessions.Commands.CheckActiveSession;

public class CheckActiveSessionCommandHandler : IRequestHandler<CheckActiveSessionCommand, Result>
{
    private readonly IGeneralRepo<WorkoutSession> _sessionRepo;
	private readonly IUnitOfWork _unitOfWork;
	public CheckActiveSessionCommandHandler(IGeneralRepo<WorkoutSession> sessionRepo, IUnitOfWork unitOfWork)
    {
        _sessionRepo = sessionRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CheckActiveSessionCommand request, CancellationToken cancellationToken)
    {
		return await _unitOfWork.ExecuteAsync(async () =>
        {
            var activeSession = await _sessionRepo.GetAll()
            .AnyAsync(s => s.UserId == request.UserId && s.Status == "Active", cancellationToken);

            if (activeSession)
                return Result.Failure(Error.Conflict(WorkoutErrorCodes.SessionAlreadyActive, "User already has an active workout session."));

            return Result.Success();
        }, cancellationToken);
    }
}
