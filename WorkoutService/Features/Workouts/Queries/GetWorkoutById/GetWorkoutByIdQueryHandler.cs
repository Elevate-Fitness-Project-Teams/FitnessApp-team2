using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkoutService.Common;
using WorkoutService.Data;
using WorkoutService.Data.Entities;

namespace WorkoutService.Features.Workouts.Queries.GetWorkoutById;

public class GetWorkoutByIdQueryHandler : IRequestHandler<GetWorkoutByIdQuery, Result<GetWorkoutByIdResponse>>
{
    private readonly IGeneralRepo<Workout> _workoutRepo;

    public GetWorkoutByIdQueryHandler(IGeneralRepo<Workout> workoutRepo)
    {
        _workoutRepo = workoutRepo;
    }

    public async Task<Result<GetWorkoutByIdResponse>> Handle(GetWorkoutByIdQuery request, CancellationToken cancellationToken)
    {
        var response = await _workoutRepo.GetAll()
            .AsNoTracking()
            .Where(w => w.Id == request.Id)
            .Select(w => new GetWorkoutByIdResponse(
                w.Id,
                w.Name,
                w.DurationInMinutes,
                w.Difficulty,
                w.WorkoutExercises
                    .OrderBy(we => we.OrderIndex)
                    .Select(we => new WorkoutExerciseDto(
                        we.ExerciseId,
                        we.Exercise!.Name,
                        we.SetsDefault,
                        we.RepsDefault,
                        we.RestTimeInSeconds,
                        we.OrderIndex
                    )).ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (response == null)
            return Result<GetWorkoutByIdResponse>.Failure(Error.NotFound(WorkoutErrorCodes.WorkoutNotFound, "Workout not found."));

        return Result<GetWorkoutByIdResponse>.Success(response);
    }
}

