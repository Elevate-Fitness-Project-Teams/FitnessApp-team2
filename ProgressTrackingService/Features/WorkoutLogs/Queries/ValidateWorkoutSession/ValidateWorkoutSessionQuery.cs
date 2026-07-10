using MediatR;
using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.WorkoutLogs.Queries.ValidateWorkoutSession;

public class ValidateWorkoutSessionQuery : IRequest<Result<bool>>
{
    public string SessionId { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}

public class ValidateWorkoutSessionHandler : IRequestHandler<ValidateWorkoutSessionQuery, Result<bool>>
{
    private readonly IGeneralRepo<WorkoutSession> _sessionRepo;

    public ValidateWorkoutSessionHandler(IGeneralRepo<WorkoutSession> sessionRepo)
    {
        _sessionRepo = sessionRepo;
    }

    public async Task<Result<bool>> Handle(ValidateWorkoutSessionQuery request, CancellationToken cancellationToken)
    {
        var exists = await _sessionRepo
            .Find(s => s.Id == request.SessionId && s.UserId == request.UserId)
            .AnyAsync(cancellationToken);
            
        return Result<bool>.Success(exists);
    }
}
