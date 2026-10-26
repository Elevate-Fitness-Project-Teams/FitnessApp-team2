using FitnessCalculationService.Common;
using FitnessCalculationService.Domain.Entities;
using FitnessCalculationService.Domain.Enums;
using FitnessCalculationService.Persistence;
using FitnessCalculationService.Persistence.Repositories;
using MediatR;
using MessageBroker.Events;

namespace FitnessCalculationService.Features.SubmitWeightGoalActivity.Commands
{
    public record SubmitFitnessStatsCommand(
        Guid UserId,
        double Weight,
        double Height,
        int Age,
        Gender Gender,
        FitnessGoal Goal,
        ActivityLevel ActivityLevel,
        string Source = "user",
        Guid? SourceEventId = null
    ) : IRequest<Result<Guid>>;

    public class SubmitFitnessStatsCommandHandler : IRequestHandler<SubmitFitnessStatsCommand, Result<Guid>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IGenericRepository<UserFitnessStats> _repo;
        private readonly MassTransit.IPublishEndpoint _publishEndpoint;

        public SubmitFitnessStatsCommandHandler(
            IUnitOfWork uow,
            IGenericRepository<UserFitnessStats> repo,
            MassTransit.IPublishEndpoint publishEndpoint)
        {
            _uow = uow;
            _repo = repo;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Result<Guid>> Handle(SubmitFitnessStatsCommand request, CancellationToken cancellationToken)
        {
            return await _uow.ExecuteAsync(async () =>
            {
                var newStats = new UserFitnessStats
                {
                    Id = Guid.CreateVersion7(),
                    UserId = request.UserId,
                    Weight = request.Weight,
                    Height = request.Height,
                    Age = request.Age,
                    Gender = request.Gender,
                    Goal = request.Goal,
                    ActivityLevel = request.ActivityLevel,
                    RecordedAt = DateTime.UtcNow,
                    Source = request.Source,
                    SourceEventId = request.SourceEventId
                };

                await _repo.AddAsync(newStats, cancellationToken);

                await _publishEndpoint.Publish(new FitnessStatsSubmittedEvent
                {
                    UserId = request.UserId,
                    HeightCm = request.Height,
                    WeightKg = request.Weight,
                    RecordedAt = newStats.RecordedAt
                }, cancellationToken);

                return Result<Guid>.Success(newStats.Id);
            }, cancellationToken);
        }
    }
}
