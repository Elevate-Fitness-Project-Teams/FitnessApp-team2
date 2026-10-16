using FitnessCalculationService.Common;
using FitnessCalculationService.Domain.Entities;
using FitnessCalculationService.Domain.Enums;
using FitnessCalculationService.Persistence.Repositories;
using MediatR;

namespace FitnessCalculationService.Features.Plans.Commands;

public record AppendPlanHistoryCommand(
    Guid UserId,
    string PlanId,
    string? PreviousPlanId,
    FitnessGoal Goal,
    CalorieStatus Status,
    CalorieStatus? PreviousStatus,
    double Bmr,
    double Tdee,
    double CalorieTarget,
    Guid? AssignedPlanId,
    string TriggeredBy,
    string Reason
) : IRequest<Result<Guid>>;

public class AppendPlanHistoryCommandHandler : IRequestHandler<AppendPlanHistoryCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    private readonly IGenericRepository<UserPlanHistory> _repo;

    public AppendPlanHistoryCommandHandler(IUnitOfWork uow, IGenericRepository<UserPlanHistory> repo)
    {
        _uow = uow;
        _repo = repo;
    }

    public async Task<Result<Guid>> Handle(AppendPlanHistoryCommand request, CancellationToken cancellationToken)
    {
        return await _uow.ExecuteAsync(async () =>
        {
            var history = new UserPlanHistory
            {
                Id = Guid.CreateVersion7(),
                UserId = request.UserId,
                PlanId = request.PlanId,
                PreviousPlanId = request.PreviousPlanId,
                Goal = request.Goal,
                Status = request.Status,
                PreviousStatus = request.PreviousStatus,
                Bmr = request.Bmr,
                Tdee = request.Tdee,
                CalorieTarget = request.CalorieTarget,
                AssignedPlanId = request.AssignedPlanId,
                TriggeredBy = request.TriggeredBy,
                Reason = request.Reason,
                AssignedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(history, cancellationToken);
            return Result<Guid>.Success(history.Id);
        }, cancellationToken);
    }
}
