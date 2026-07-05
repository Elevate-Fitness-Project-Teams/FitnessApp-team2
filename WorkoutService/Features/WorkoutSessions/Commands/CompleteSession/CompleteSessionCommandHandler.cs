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
            var status = await _sessionRepo.GetAll()
                .Where(s => s.SessionId == request.SessionId && s.UserId == request.UserId)
                .Select(s => s.Status)
                .FirstOrDefaultAsync(cancellationToken);

            if (status == null)
                return Result.Failure(Error.NotFound("SESSION_NOT_FOUND", "Workout session not found or does not belong to the user."));

            if (status != "Active")
                return Result.Failure(Error.Conflict("SESSION_NOT_ACTIVE", "Workout session is not active."));

            await _sessionRepo.GetAll()
                .Where(s => s.SessionId == request.SessionId && s.UserId == request.UserId)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, "Completed"), cancellationToken);

            return Result.Success();
        }, cancellationToken);
	}
}
