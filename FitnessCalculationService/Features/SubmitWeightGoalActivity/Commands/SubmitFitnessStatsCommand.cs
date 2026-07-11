using FitnessCalculationService.Common;
using FitnessCalculationService.Domain.Entities;
using FitnessCalculationService.Domain.Enums;
using FitnessCalculationService.Persistence;
using FitnessCalculationService.Persistence.Repositories;
using MediatR;

namespace FitnessCalculationService.Features.SubmitWeightGoalActivity.Commands
{
    public record SubmitFitnessStatsCommand(
        String UserId,
        double Weight,
        double Height,
        int Age,
        Gender Gender,
        FitnessGoal Goal,
        ActivityLevel ActivityLevel
    ) : IRequest<Result<Guid>>;
    public class SubmitFitnessStatsCommandHandler : IRequestHandler<SubmitFitnessStatsCommand, Result<Guid>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IGenericRepository<UserFitnessStats> _repo;

        public SubmitFitnessStatsCommandHandler(IUnitOfWork uow, IGenericRepository<UserFitnessStats> Repo)
        {
            _uow = uow;
            _repo = Repo;
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
                    RecordedAt = DateTime.UtcNow
                };

                await _repo.AddAsync(newStats, cancellationToken);

                return Result<Guid>.Success(newStats.Id);

            }, cancellationToken);
        }
    }
}
