using MediatR;
using WorkoutService.Common;
using WorkoutService.Data;
using WorkoutService.Features.WorkoutSessions.Queries.CheckActiveSession;
using WorkoutService.Features.WorkoutSessions.Queries.CheckWorkoutExists;
using WorkoutService.Features.WorkoutSessions.Commands.CreateSession;

namespace WorkoutService.Features.WorkoutSessions.Orchestrators.StartSession;

public class StartSessionOrchestratorHandler : IRequestHandler<StartSessionOrchestrator, Result<StartSessionResponse>>
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public StartSessionOrchestratorHandler(IMediator mediator, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<StartSessionResponse>> Handle(StartSessionOrchestrator request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            var workoutResult = await _mediator.Send(new CheckWorkoutExistsQuery(request.WorkoutId), cancellationToken);
            if (workoutResult.IsFailure)
                return Result<StartSessionResponse>.Failure(workoutResult.Errors);

            var activeSessionResult = await _mediator.Send(new CheckActiveSessionQuery(request.UserId), cancellationToken);
            if (activeSessionResult.IsFailure)
                return Result<StartSessionResponse>.Failure(activeSessionResult.Errors);

            var createResult = await _mediator.Send(new CreateSessionCommand(request.UserId, request.WorkoutId), cancellationToken);
            if (createResult.IsFailure)
                return Result<StartSessionResponse>.Failure(createResult.Errors);

            return Result<StartSessionResponse>.Success(createResult.Value);
        }, cancellationToken);
    }
}

