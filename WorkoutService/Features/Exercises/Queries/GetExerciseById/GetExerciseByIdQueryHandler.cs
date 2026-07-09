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
		var response = await _exerciseRepo.GetAll()
			.AsNoTracking().
			Where(e => e.Id == request.Id)
			.Select(e => new GetExerciseByIdResponse(
				e.Id,
				e.Name,
				e.TargetMuscles,
				e.EquipmentNeeded,
				e.Description,
				e.VideoUrl
				))
			.FirstOrDefaultAsync(cancellationToken);

		if (response == null)
			return Result<GetExerciseByIdResponse>.Failure(Error.NotFound(WorkoutErrorCodes.ExerciseNotFound, "Exercise not found."));

	

		return Result<GetExerciseByIdResponse>.Success(response);
	}
}
