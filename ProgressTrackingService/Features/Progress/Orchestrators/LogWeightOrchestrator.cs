using MassTransit;
using MediatR;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Features.Progress.Commands.LogWeight;
using ProgressTrackingService.Features.Progress.Queries.GetPreviousWeight;
using ProgressTrackingService.MessageBroker.Events;

namespace ProgressTrackingService.Features.Progress.Orchestrators;

public record LogWeightOrchestrator(Guid UserId, double Weight, DateTime Date, string? Notes) 
    : IRequest<Result>;

public class LogWeightOrchestratorHandler : IRequestHandler<LogWeightOrchestrator, Result>
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;

    public LogWeightOrchestratorHandler(IMediator mediator, IUnitOfWork unitOfWork, IPublishEndpoint publishEndpoint)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result> Handle(LogWeightOrchestrator request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            // 1. Get previous weight
            var previousWeightResult = await _mediator.Send(new GetPreviousWeightQuery(request.UserId), cancellationToken);
            double previousWeight = previousWeightResult.Value ?? request.Weight;
            
            // 2. Compute difference
            double differenceFromPrevious = previousWeightResult.Value.HasValue 
                ? request.Weight - previousWeight 
                : 0;

            // 3. Save weight entry
            var saveResult = await _mediator.Send(new SaveWeightEntryCommand
            {
                UserId = request.UserId,
                Weight = request.Weight,
                Date = request.Date,
                Notes = request.Notes
            }, cancellationToken);

            if (!saveResult.IsSuccess)
                return Result.Failure(saveResult.Error);
            

            // 4. Update stats
            var statsResult = await _mediator.Send(new UpdateWeightStatisticCommand
            {
                UserId = request.UserId,
                WeightDifference = differenceFromPrevious
            }, cancellationToken);

            if (!statsResult.IsSuccess)
                return Result.Failure(statsResult.Error);
            

            // 5. Publish event via Outbox
            await _publishEndpoint.Publish(new WeightUpdatedEvent
            {
                UserId = request.UserId,
                NewWeight = request.Weight,
                RecordedAt = request.Date
            }, cancellationToken);

            return Result.Success();
        }, cancellationToken);
    }
}
