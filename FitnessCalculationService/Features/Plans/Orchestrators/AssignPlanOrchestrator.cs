using FitnessCalculationService.Common;
using FitnessCalculationService.Common.Errors;
using FitnessCalculationService.Domain.Entities;
using FitnessCalculationService.Features.Plans.Commands;
using FitnessCalculationService.Features.Plans.Queries;
using FitnessCalculationService.Persistence.Repositories;
using MassTransit;
using MediatR;
using MessageBroker.Events;
using Microsoft.EntityFrameworkCore;

namespace FitnessCalculationService.Features.Plans.Orchestrators;

public record AssignPlanOrchestrator(
    Guid UserId, 
    string? Reason = null, 
    string TriggeredBy = "user"
) : IRequest<Result<AssignPlanResponse>>;

public class AssignPlanOrchestratorHandler : IRequestHandler<AssignPlanOrchestrator, Result<AssignPlanResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly IGenericRepository<CalculatedMetrics> _metricsRepo;
    private readonly IGenericRepository<UserFitnessStats> _statsRepo;
    private readonly IMediator _mediator;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<AssignPlanOrchestratorHandler> _logger;

    public AssignPlanOrchestratorHandler(
        IUnitOfWork uow,
        IGenericRepository<CalculatedMetrics> metricsRepo,
        IGenericRepository<UserFitnessStats> statsRepo,
        IMediator mediator,
        IPublishEndpoint publishEndpoint,
        ILogger<AssignPlanOrchestratorHandler> logger)
    {
        _uow = uow;
        _metricsRepo = metricsRepo;
        _statsRepo = statsRepo;
        _mediator = mediator;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<Result<AssignPlanResponse>> Handle(AssignPlanOrchestrator request, CancellationToken cancellationToken)
    {
        // 1. Check existing calculated metrics
        var metrics = await _metricsRepo.GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.UserId == request.UserId, cancellationToken);

        if (metrics == null)
        {
            return Result<AssignPlanResponse>.Failure(new Error(ErrorCodes.FceMetricsNotCalculated, "No calculated metrics found. Please calculate metrics before assigning a plan."));
        }

        // 2. Fetch latest fitness stats to obtain Goal
        var latestStats = await _statsRepo.GetQueryable()
            .AsNoTracking()
            .Where(s => s.UserId == request.UserId)
            .OrderByDescending(s => s.RecordedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestStats == null)
        {
            return Result<AssignPlanResponse>.Failure(new Error(ErrorCodes.FceStatsNotFound, "No fitness stats found for user."));
        }

        // 3. Find matching plan configuration by (Goal, Status)
        var planConfigResult = await _mediator.Send(new GetPlanConfigByGoalStatusQuery(latestStats.Goal, metrics.Status), cancellationToken);
        if (!planConfigResult.IsSuccess || planConfigResult.Value == null)
        {
            return Result<AssignPlanResponse>.Failure(planConfigResult.Error ?? new Error(ErrorCodes.FceNoMatchingPlan, "No active plan matches user goal and calorie status."));
        }
        var planConfig = planConfigResult.Value;

        // 4. Check currently active plan (if any)
        var activePlanResult = await _mediator.Send(new GetActiveAssignedPlanQuery(request.UserId), cancellationToken);
        var activePlan = activePlanResult.Value;

        var effectiveReason = !string.IsNullOrWhiteSpace(request.Reason)
            ? request.Reason
            : (activePlan == null ? "initial_assignment" : "manual_reassignment");

        // 5. Execute transactional mutation (UoW)
        Guid newAssignmentId = Guid.Empty;
        var transactionResult = await _uow.ExecuteAsync(async () =>
        {
            // Deactivate any currently active plan
            var deactivateResult = await _mediator.Send(new DeactivateActivePlanCommand(request.UserId), cancellationToken);
            if (!deactivateResult.IsSuccess)
            {
                return Result.Rollback(deactivateResult.Error ?? new Error("FCE_DEACTIVATE_FAILED", "Failed to deactivate active plan."));
            }

            // Create new active plan assignment
            var createResult = await _mediator.Send(new CreateAssignedPlanCommand(request.UserId, planConfig.Id), cancellationToken);
            if (!createResult.IsSuccess)
            {
                return Result.Rollback(createResult.Error ?? new Error("FCE_ASSIGN_FAILED", "Failed to create new plan assignment."));
            }
            newAssignmentId = createResult.Value;

            // Append immutable snapshot to history
            var historyResult = await _mediator.Send(new AppendPlanHistoryCommand(
                request.UserId,
                planConfig.PlanId,
                activePlan?.PlanId,
                latestStats.Goal,
                metrics.Status,
                null,
                metrics.Bmr,
                metrics.Tdee,
                metrics.CalorieTarget,
                newAssignmentId,
                request.TriggeredBy,
                effectiveReason
            ), cancellationToken);

            if (!historyResult.IsSuccess)
            {
                return Result.Rollback(historyResult.Error ?? new Error("FCE_HISTORY_FAILED", "Failed to record plan assignment history."));
            }

            // 6. Stage domain events in Transactional Outbox (persisted atomically with UoW commit)
            await _publishEndpoint.Publish(new FitnessPlanAssignedEvent
            {
                UserId = request.UserId,
                PlanId = planConfig.PlanId,
                PreviousPlanId = activePlan?.PlanId,
                Reason = effectiveReason,
                AssignedAt = DateTime.UtcNow
            }, cancellationToken);

            await _publishEndpoint.Publish(new PlanAssignedEvent
            {
                UserId = request.UserId,
                PlanId = planConfig.PlanId,
                AssignedAt = DateTime.UtcNow
            }, cancellationToken);

            return Result.Success();
        }, cancellationToken);

        if (!transactionResult.IsSuccess)
        {
            return Result<AssignPlanResponse>.Failure(transactionResult.Error);
        }

        var response = new AssignPlanResponse(
            newAssignmentId,
            planConfig.PlanId,
            planConfig.Name,
            planConfig.Description,
            planConfig.Goal.ToString(),
            planConfig.Status.ToString(),
            planConfig.EstimatedDurationWeeks,
            planConfig.WorkoutsPerWeek,
            planConfig.ProgramType,
            planConfig.MinCalorie,
            planConfig.MaxCalorie,
            DateTime.UtcNow
        );

        return Result<AssignPlanResponse>.Success(response);
    }
}
