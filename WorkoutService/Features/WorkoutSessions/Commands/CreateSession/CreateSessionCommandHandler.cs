using MediatR;
using WorkoutService.Common;
using WorkoutService.Data;
using WorkoutService.Data.Entities;
using WorkoutService.Features.WorkoutSessions.Orchestrators.StartSession;

namespace WorkoutService.Features.WorkoutSessions.Commands.CreateSession;

public class CreateSessionCommandHandler : IRequestHandler<CreateSessionCommand, Result>
{
    private readonly IGeneralRepo<WorkoutSession> _sessionRepo;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSessionCommandHandler(IGeneralRepo<WorkoutSession> sessionRepo, IUnitOfWork unitOfWork)
    {
        _sessionRepo = sessionRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
    {
		return await _unitOfWork.ExecuteAsync(async () =>
        {
            var session = new WorkoutSession
            {
                SessionId = Guid.NewGuid().ToString(),
                UserId = request.UserId,
                WorkoutId = request.WorkoutId,
                StartedAt = DateTime.UtcNow,
                Status = "Active"
            };

            await _sessionRepo.AddAsync(session, cancellationToken);

            return Result.Success();
        }, cancellationToken);
    }
}

