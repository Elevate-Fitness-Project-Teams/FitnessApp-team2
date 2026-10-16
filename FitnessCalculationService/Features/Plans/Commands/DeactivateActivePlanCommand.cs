using FitnessCalculationService.Common;
using FitnessCalculationService.Domain.Entities;
using FitnessCalculationService.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitnessCalculationService.Features.Plans.Commands;

public record DeactivateActivePlanCommand(Guid UserId) : IRequest<Result>;

public class DeactivateActivePlanCommandHandler : IRequestHandler<DeactivateActivePlanCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly IGenericRepository<UserAssignedPlan> _repo;

    public DeactivateActivePlanCommandHandler(IUnitOfWork uow, IGenericRepository<UserAssignedPlan> repo)
    {
        _uow = uow;
        _repo = repo;
    }

    public async Task<Result> Handle(DeactivateActivePlanCommand request, CancellationToken cancellationToken)
    {
        return await _uow.ExecuteAsync(async () =>
        {
            await _repo.GetQueryable()
                .Where(p => p.UserId == request.UserId && p.IsActive)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsActive, false), cancellationToken);

            return Result.Success();
        }, cancellationToken);
    }
}
