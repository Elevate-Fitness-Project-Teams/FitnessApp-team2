using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkoutService.Common;
using WorkoutService.Data;
using WorkoutService.Data.Entities;

namespace WorkoutService.Features.WorkoutSessions.Commands.StartSession;

public class StartSessionCommandHandler : IRequestHandler<StartSessionCommand, Result<StartSessionResponse>>
{
    private readonly IGeneralRepo<Workout> _workoutRepo;
    private readonly IGeneralRepo<WorkoutSession> _sessionRepo;
    private readonly IUnitOfWork _unitOfWork;

    public StartSessionCommandHandler(IGeneralRepo<Workout> workoutRepo, IGeneralRepo<WorkoutSession> sessionRepo, IUnitOfWork unitOfWork)
    {
        _workoutRepo = workoutRepo;
        _sessionRepo = sessionRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<StartSessionResponse>> Handle(StartSessionCommand request, CancellationToken cancellationToken)
    {
        var workoutExists = await _workoutRepo.GetAll().AnyAsync(w => w.Id == request.WorkoutId, cancellationToken);
        if (!workoutExists)
        {
            return Result<StartSessionResponse>.Failure(Error.NotFound(WorkoutErrorCodes.WorkoutNotFound, "Workout not found."));
        }

        var activeSession = await _sessionRepo.GetAll()
            .FirstOrDefaultAsync(s => s.UserId == request.UserId && s.Status == "Active", cancellationToken);
            
        if (activeSession != null)
        {
            return Result<StartSessionResponse>.Failure(Error.Conflict(WorkoutErrorCodes.SessionAlreadyActive, "User already has an active workout session."));
        }

        var session = new WorkoutSession
        {
            SessionId = Guid.NewGuid().ToString(),
            UserId = request.UserId,
            WorkoutId = request.WorkoutId,
            StartedAt = DateTime.UtcNow,
            Status = "Active"
        };

        await _sessionRepo.AddAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<StartSessionResponse>.Success(new StartSessionResponse(session.SessionId, session.StartedAt));
    }
}
