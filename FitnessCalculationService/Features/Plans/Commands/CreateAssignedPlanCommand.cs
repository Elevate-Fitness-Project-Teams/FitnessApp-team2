using FitnessCalculationService.Common;
using FitnessCalculationService.Domain.Entities;
using FitnessCalculationService.Persistence.Repositories;
using MediatR;

namespace FitnessCalculationService.Features.Plans.Commands;

public record CreateAssignedPlanCommand(Guid UserId, Guid FitnessPlanConfigId) : IRequest<Result<Guid>>;

public class CreateAssignedPlanCommandHandler : IRequestHandler<CreateAssignedPlanCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    private readonly IGenericRepository<UserAssignedPlan> _repo;

    public CreateAssignedPlanCommandHandler(IUnitOfWork uow, IGenericRepository<UserAssignedPlan> repo)
    {
        _uow = uow;
        _repo = repo;
    }

    public async Task<Result<Guid>> Handle(CreateAssignedPlanCommand request, CancellationToken cancellationToken)
    {
        return await _uow.ExecuteAsync(async () =>
        {
            var plan = new UserAssignedPlan
            {
                Id = Guid.CreateVersion7(),
                UserId = request.UserId,
                FitnessPlanConfigId = request.FitnessPlanConfigId,
                IsActive = true,
                AssignedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(plan, cancellationToken);
            return Result<Guid>.Success(plan.Id);
        }, cancellationToken);
    }
}
