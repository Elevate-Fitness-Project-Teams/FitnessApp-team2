using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkoutService.Common;
using WorkoutService.Data;
using WorkoutService.Data.Entities;

namespace WorkoutService.Features.WorkoutPlans.Queries.GetWorkoutPlanById;

public class GetWorkoutPlanByIdQueryHandler : IRequestHandler<GetWorkoutPlanByIdQuery, Result<GetWorkoutPlanByIdResponse>>
{
	private readonly IGeneralRepo<WorkoutPlan> _workoutPlanRepo;

	public GetWorkoutPlanByIdQueryHandler(IGeneralRepo<WorkoutPlan> workoutPlanRepo)
	{
		_workoutPlanRepo = workoutPlanRepo;
	}

	public async Task<Result<GetWorkoutPlanByIdResponse>> Handle(GetWorkoutPlanByIdQuery request, CancellationToken cancellationToken)
	{
		var response = await _workoutPlanRepo.GetAll()
			.AsNoTracking()
			.Where(p => p.ExternalPlanId == request.PlanId)
			.Select(p => new GetWorkoutPlanByIdResponse(
				p.Id,
				p.ExternalPlanId,
				p.Name,
				p.Description,
				p.Goal,
				p.Status,
				p.Difficulty
			))
			.FirstOrDefaultAsync(cancellationToken);

		if (response == null)
			return Result<GetWorkoutPlanByIdResponse>.Failure(Error.NotFound(WorkoutErrorCodes.WorkoutPlanNotFound, "Workout plan not found."));

		return Result<GetWorkoutPlanByIdResponse>.Success(response);
	}
}

