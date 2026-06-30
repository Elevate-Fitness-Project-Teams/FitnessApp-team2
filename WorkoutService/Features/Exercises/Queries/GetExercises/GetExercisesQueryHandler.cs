using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkoutService.Common;
using WorkoutService.Data;
using WorkoutService.Data.Entities;

namespace WorkoutService.Features.Exercises.Queries.GetExercises;

public class GetExercisesQueryHandler : IRequestHandler<GetExercisesQuery, Result<IEnumerable<GetExercisesResponse>>>
{
    private readonly IGeneralRepo<Exercise> _exerciseRepo;

    public GetExercisesQueryHandler(IGeneralRepo<Exercise> exerciseRepo)
    {
        _exerciseRepo = exerciseRepo;
    }

    public async Task<Result<IEnumerable<GetExercisesResponse>>> Handle(GetExercisesQuery request, CancellationToken cancellationToken)
    {
        var exercises = await _exerciseRepo.GetAll()
            .AsNoTracking()
            .Select(e => new GetExercisesResponse(
                e.Id,
                e.Name,
                e.TargetMuscles,
                e.EquipmentNeeded,
                e.Description,
                e.VideoUrl
            ))
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<GetExercisesResponse>>.Success(exercises);
    }
}
