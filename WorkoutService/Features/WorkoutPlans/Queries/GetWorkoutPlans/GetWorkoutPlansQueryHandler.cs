using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkoutService.Common;
using WorkoutService.Data;
using WorkoutService.Data.Entities;

namespace WorkoutService.Features.WorkoutPlans.Queries.GetWorkoutPlans;

public class GetWorkoutPlansQueryHandler : IRequestHandler<GetWorkoutPlansQuery, Result<IEnumerable<GetWorkoutPlansResponse>>>
{
    private readonly IGeneralRepo<WorkoutPlan> _workoutPlanRepo;

    public GetWorkoutPlansQueryHandler(IGeneralRepo<WorkoutPlan> workoutPlanRepo)
    {
        _workoutPlanRepo = workoutPlanRepo;
    }

    public async Task<Result<IEnumerable<GetWorkoutPlansResponse>>> Handle(GetWorkoutPlansQuery request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var skip = (Math.Max(request.Page, 1) - 1) * pageSize;

        var plans = await _workoutPlanRepo.GetAll()
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .Skip(skip)
            .Take(pageSize)
            .Select(p => new GetWorkoutPlansResponse(
                p.Id,
                p.ExternalPlanId,
                p.Name,
                p.Description,
                p.Goal,
                p.Status,
                p.Difficulty
            ))
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<GetWorkoutPlansResponse>>.Success(plans);
    }
}
