using MediatR;
using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.WorkoutLogs.Queries.GetWorkoutLogBySession;

public record GetWorkoutLogBySessionQuery(string SessionId, Guid UserId) : IRequest<Result<Guid?>>;

public class GetWorkoutLogBySessionHandler : IRequestHandler<GetWorkoutLogBySessionQuery, Result<Guid?>>
{
    private readonly IGeneralRepo<WorkoutLog> _repo;

    public GetWorkoutLogBySessionHandler(IGeneralRepo<WorkoutLog> repo)
    {
        _repo = repo;
    }

    public async Task<Result<Guid?>> Handle(GetWorkoutLogBySessionQuery request, CancellationToken cancellationToken)
    {
        var logId = await _repo.Find(w => w.SessionId == request.SessionId && w.UserId == request.UserId)
            .Select(w => (Guid?)w.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return Result.Success(logId);
    }
}
