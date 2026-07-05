using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkoutService.Common;
using WorkoutService.Data;
using WorkoutService.Data.Entities;

namespace WorkoutService.Features.WorkoutSessions.Queries.CheckWorkoutExists;

public class CheckWorkoutExistsQueryHandler : IRequestHandler<CheckWorkoutExistsQuery, Result>
{
    private readonly IGeneralRepo<Workout> _workoutRepo;
	private readonly IUnitOfWork _unitOfWork;

	public CheckWorkoutExistsQueryHandler(IGeneralRepo<Workout> workoutRepo, IUnitOfWork unitOfWork)
    {
        _workoutRepo = workoutRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CheckWorkoutExistsQuery request, CancellationToken cancellationToken)
    {
		return await _unitOfWork.ExecuteAsync(async () =>
		{
			var exists = await _workoutRepo.GetAll()
            .AnyAsync(w => w.Id == request.WorkoutId, cancellationToken);

        if (!exists)
            return Result.Failure(Error.NotFound(WorkoutErrorCodes.WorkoutNotFound, "Workout not found."));

        return Result.Success();
		}, cancellationToken);
	}
}
