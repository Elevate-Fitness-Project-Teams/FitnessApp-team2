using MediatR;
using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.Progress.Queries.ViewUserProgress;

public record CheckUserExistsQuery(Guid UserId) : IRequest<Result<bool>>;

public class CheckUserExistsQueryHandler : IRequestHandler<CheckUserExistsQuery, Result<bool>>
{
    private readonly IGeneralRepo<Streak> _streakRepo;
    private readonly IGeneralRepo<UserStatistic> _userStatsRepo;
    private readonly IGeneralRepo<WeightHistory> _weightRepo;
    private readonly IGeneralRepo<WorkoutLog> _workoutLogRepo;

    public CheckUserExistsQueryHandler(
        IGeneralRepo<Streak> streakRepo,
        IGeneralRepo<UserStatistic> userStatsRepo,
        IGeneralRepo<WeightHistory> weightRepo,
        IGeneralRepo<WorkoutLog> workoutLogRepo)
    {
        _streakRepo = streakRepo;
        _userStatsRepo = userStatsRepo;
        _weightRepo = weightRepo;
        _workoutLogRepo = workoutLogRepo;
    }

    public async Task<Result<bool>> Handle(CheckUserExistsQuery request, CancellationToken cancellationToken)
    {
        var exists = await _streakRepo.Find(s => s.UserId == request.UserId).AnyAsync(cancellationToken)
            || await _userStatsRepo.Find(s => s.UserId == request.UserId).AnyAsync(cancellationToken)
            || await _weightRepo.Find(w => w.UserId == request.UserId).AnyAsync(cancellationToken)
            || await _workoutLogRepo.Find(w => w.UserId == request.UserId).AnyAsync(cancellationToken);

        return Result<bool>.Success(exists);
    }
}
