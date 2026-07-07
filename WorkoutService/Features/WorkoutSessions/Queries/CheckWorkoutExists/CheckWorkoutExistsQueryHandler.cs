using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkoutService.Common;
using WorkoutService.Data;
using WorkoutService.Data.Entities;

namespace WorkoutService.Features.WorkoutSessions.Queries.CheckWorkoutExists;

public class CheckWorkoutExistsQueryHandler : IRequestHandler<CheckWorkoutExistsQuery, Result>
{
	private readonly IGeneralRepo<Workout> _workoutRepo;

	public CheckWorkoutExistsQueryHandler(IGeneralRepo<Workout> workoutRepo)
	{
		_workoutRepo = workoutRepo;
	}

	public async Task<Result> Handle(CheckWorkoutExistsQuery request, CancellationToken cancellationToken)
	{

		var exists = await _workoutRepo.GetAll()
		.AnyAsync(w => w.Id == request.WorkoutId, cancellationToken);

		if (!exists)
			return Result.Failure(Error.NotFound(WorkoutErrorCodes.WorkoutNotFound, "Workout not found."));

		return Result.Success();
	}
}
