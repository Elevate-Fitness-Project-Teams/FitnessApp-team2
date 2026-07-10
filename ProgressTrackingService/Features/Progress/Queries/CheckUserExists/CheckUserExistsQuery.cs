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

    public CheckUserExistsQueryHandler(IGeneralRepo<Streak> streakRepo)
    {
        _streakRepo = streakRepo;
    }

    public async Task<Result<bool>> Handle(CheckUserExistsQuery request, CancellationToken cancellationToken)
    {
        var exists = await _streakRepo.Find(s => s.UserId == request.UserId).AnyAsync(cancellationToken);
        return Result<bool>.Success(exists);
    }
}
