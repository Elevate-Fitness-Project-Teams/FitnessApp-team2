using MediatR;
using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.WorkoutLogs.Commands.UpdateSession;

public class UpdateSessionCommand : IRequest<Result>
{
    public string SessionId { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}

public class UpdateSessionHandler : IRequestHandler<UpdateSessionCommand, Result>
{
    private readonly IGeneralRepo<WorkoutSession> _sessionRepo;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSessionHandler(IGeneralRepo<WorkoutSession> sessionRepo, IUnitOfWork unitOfWork)
    {
        _sessionRepo = sessionRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateSessionCommand request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
        var session = await _sessionRepo.Find(s => s.Id == request.SessionId && s.UserId == request.UserId)
            .Select(s => new WorkoutSession { Id = s.Id, Status = s.Status })
            .FirstOrDefaultAsync(cancellationToken);

        if (session != null)
        {
            session.Status = "Completed";
            _sessionRepo.SaveInclude(session, nameof(WorkoutSession.Status));
        }

        return Result.Success();
        }, cancellationToken);
    }
}
