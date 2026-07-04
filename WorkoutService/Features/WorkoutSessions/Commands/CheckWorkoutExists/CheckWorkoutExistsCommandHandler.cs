using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkoutService.Common;
using WorkoutService.Data;
using WorkoutService.Data.Entities;

namespace WorkoutService.Features.WorkoutSessions.Commands.CheckWorkoutExists;

public class CheckWorkoutExistsCommandHandler : IRequestHandler<CheckWorkoutExistsCommand, Result>
{
    private readonly IGeneralRepo<Workout> _workoutRepo;
	private readonly IUnitOfWork _unitOfWork;

	public CheckWorkoutExistsCommandHandler(IGeneralRepo<Workout> workoutRepo, IUnitOfWork unitOfWork)
    {
        _workoutRepo = workoutRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CheckWorkoutExistsCommand request, CancellationToken cancellationToken)
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
