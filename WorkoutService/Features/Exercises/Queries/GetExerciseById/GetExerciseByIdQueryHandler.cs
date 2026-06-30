using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkoutService.Common;
using WorkoutService.Data;
using WorkoutService.Data.Entities;

namespace WorkoutService.Features.Exercises.Queries.GetExerciseById;

public class GetExerciseByIdQueryHandler : IRequestHandler<GetExerciseByIdQuery, Result<GetExerciseByIdResponse>>
{
    private readonly IGeneralRepo<Exercise> _exerciseRepo;

    public GetExerciseByIdQueryHandler(IGeneralRepo<Exercise> exerciseRepo)
    {
        _exerciseRepo = exerciseRepo;
    }

    public async Task<Result<GetExerciseByIdResponse>> Handle(GetExerciseByIdQuery request, CancellationToken cancellationToken)
    {
        var exercise = await _exerciseRepo.GetAll()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (exercise == null)
            return Result<GetExerciseByIdResponse>.Failure(Error.NotFound(WorkoutErrorCodes.ExerciseNotFound, "Exercise not found."));

        var response = new GetExerciseByIdResponse(
            exercise.Id,
            exercise.Name,
            exercise.TargetMuscles,
            exercise.EquipmentNeeded,
            exercise.Description,
            exercise.VideoUrl
        );

        return Result<GetExerciseByIdResponse>.Success(response);
    }
}
