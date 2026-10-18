using FitnessCalculationService.Common;
using FitnessCalculationService.Common.Caching;
using FitnessCalculationService.Common.Errors;
using FitnessCalculationService.Domain.Entities;
using FitnessCalculationService.Domain.Services;
using FitnessCalculationService.Features.CalculateFitnessMetrics.Commands;
using FitnessCalculationService.Features.Plans.Commands;
using FitnessCalculationService.Features.Plans.Queries;
using FitnessCalculationService.Features.SubmitWeightGoalActivity.Commands;
using FitnessCalculationService.Persistence.Repositories;
using MassTransit;
using MediatR;
using MessageBroker.Events;
using Microsoft.EntityFrameworkCore;

namespace FitnessCalculationService.Features.Calculations.Commands.RecalculateMetrics;

public record RecalculateMetricsOrchestrator(
    Guid UserId,
    double? NewWeight = null,
    string? Reason = null,
    string TriggeredBy = "user",
    DateTime? EventRecordedAt = null,
    Guid? SourceEventId = null
) : IRequest<Result<RecalculateMetricsResponse>>;

public class RecalculateMetricsOrchestratorHandler : IRequestHandler<RecalculateMetricsOrchestrator, Result<RecalculateMetricsResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly IGenericRepository<CalculatedMetrics> _metricsRepo;
    private readonly IGenericRepository<UserFitnessStats> _statsRepo;
    private readonly IMetabolicCalculator _calculator;
    private readonly IMediator _mediator;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<RecalculateMetricsOrchestratorHandler> _logger;
    private readonly ICacheService _cache;

    public RecalculateMetricsOrchestratorHandler(
        IUnitOfWork uow,
        IGenericRepository<CalculatedMetrics> metricsRepo,
        IGenericRepository<UserFitnessStats> statsRepo,
        IMetabolicCalculator calculator,
        IMediator mediator,
        IPublishEndpoint publishEndpoint,
        ILogger<RecalculateMetricsOrchestratorHandler> logger,
        ICacheService cache)
    {
        _uow = uow;
        _metricsRepo = metricsRepo;
        _statsRepo = statsRepo;
        _calculator = calculator;
        _mediator = mediator;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
        _cache = cache;
    }

    public async Task<Result<RecalculateMetricsResponse>> Handle(RecalculateMetricsOrchestrator request, CancellationToken cancellationToken)
    {
        // 1. Fetch current calculated metrics
        var currentMetrics = await _metricsRepo.GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.UserId == request.UserId, cancellationToken);

        if (currentMetrics == null)
        {
            return Result<RecalculateMetricsResponse>.Failure(
                new Error(ErrorCodes.FceMetricsNotCalculated, "No calculated metrics found for user. Initial calculation is required."));
        }

        // 2. Watermark idempotency guard for distributed RabbitMQ events
        if (request.EventRecordedAt.HasValue && 
            currentMetrics.LastWeightEventAt.HasValue && 
            request.EventRecordedAt.Value <= currentMetrics.LastWeightEventAt.Value)
        {
            _logger.LogInformation("Skipping out-of-order weight event for User {UserId}. Event timestamp {EventAt} <= LastWatermark {Watermark}",
                request.UserId, request.EventRecordedAt.Value, currentMetrics.LastWeightEventAt.Value);

            return Result<RecalculateMetricsResponse>.Success(new RecalculateMetricsResponse(
                request.UserId,
                currentMetrics.Status.ToString(),
                currentMetrics.Status.ToString(),
                currentMetrics.Bmr,
                currentMetrics.Tdee,
                currentMetrics.CalorieTarget,
                false,
                null,
                null,
                currentMetrics.CalculatedAt
            ));
        }

        // 3. Fetch latest biometrics snapshot
        var latestStats = await _statsRepo.GetQueryable()
            .AsNoTracking()
            .Where(s => s.UserId == request.UserId)
            .OrderByDescending(s => s.RecordedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestStats == null)
        {
            return Result<RecalculateMetricsResponse>.Failure(
                new Error(ErrorCodes.FceStatsNotFound, "No fitness stats found for user."));
        }

        var effectiveWeight = request.NewWeight ?? latestStats.Weight;
        Guid? newStatsId = null;

        // 4. Calculate new metabolic targets using domain engine
        var metabolicResult = _calculator.Calculate(
            effectiveWeight,
            latestStats.Height,
            latestStats.Age,
            latestStats.Gender,
            latestStats.ActivityLevel,
            latestStats.Goal
        );

        if (metabolicResult == null)
        {
            return Result<RecalculateMetricsResponse>.Failure(
                new Error(ErrorCodes.FceInvalidCalculation, "Calculation produced non-finite or invalid numbers."));
        }

        var previousStatus = currentMetrics.Status;
        var newStatus = metabolicResult.Status;
        bool statusChanged = previousStatus != newStatus;

        string? assignedPlanId = null;
        string? assignedPlanName = null;
        string? previousPlanId = null;
        bool planReassigned = false;

        // 5. Execute atomic database update using Unit of Work
        var transactionResult = await _uow.ExecuteAsync(async () =>
        {
            // Record new stats entry if weight shifted
            if (request.NewWeight.HasValue && Math.Abs(request.NewWeight.Value - latestStats.Weight) > 0.001)
            {
                var statsCommand = new SubmitFitnessStatsCommand(
                    request.UserId,
                    request.NewWeight.Value,
                    latestStats.Height,
                    latestStats.Age,
                    latestStats.Gender,
                    latestStats.Goal,
                    latestStats.ActivityLevel,
                    request.TriggeredBy,
                    request.SourceEventId
                );

                var statsResult = await _mediator.Send(statsCommand, cancellationToken);
                if (!statsResult.IsSuccess)
                {
                    return Result.Rollback(statsResult.Error ?? new Error("FCE_STATS_RECORD_FAILED", "Failed to record updated weight."));
                }
                newStatsId = statsResult.Value;
            }

            // Update CalculatedMetrics
            var updateMetricsCommand = new CalculateFitnessMetricsCommand(
                request.UserId,
                metabolicResult.Bmr,
                metabolicResult.Tdee,
                metabolicResult.CalorieTarget,
                metabolicResult.Status,
                newStatsId ?? latestStats.Id,
                request.EventRecordedAt ?? DateTime.UtcNow
            );

            var updateMetricsResult = await _mediator.Send(updateMetricsCommand, cancellationToken);
            if (!updateMetricsResult.IsSuccess)
            {
                return Result.Rollback(updateMetricsResult.Error ?? new Error("FCE_METRICS_UPDATE_FAILED", "Failed to update calculated metrics."));
            }

            // If Caloric Status threshold changed (e.g. Normal -> Hard or Weak), automatically reassign matching workout plan
            if (statusChanged)
            {
                var newPlanConfigResult = await _mediator.Send(new GetPlanConfigByGoalStatusQuery(latestStats.Goal, newStatus), cancellationToken);
                if (!newPlanConfigResult.IsSuccess || newPlanConfigResult.Value == null)
                {
                    return Result.Rollback(newPlanConfigResult.Error ?? new Error(ErrorCodes.FceNoMatchingPlan, "No active plan found for newly calculated status tier."));
                }

                var newPlanConfig = newPlanConfigResult.Value;
                assignedPlanId = newPlanConfig.PlanId;
                assignedPlanName = newPlanConfig.Name;

                var activePlanResult = await _mediator.Send(new GetActiveAssignedPlanQuery(request.UserId), cancellationToken);
                var activePlan = activePlanResult.Value;
                previousPlanId = activePlan?.PlanId;

                // Deactivate old active plan
                var deactivateResult = await _mediator.Send(new DeactivateActivePlanCommand(request.UserId), cancellationToken);
                if (!deactivateResult.IsSuccess)
                {
                    return Result.Rollback(deactivateResult.Error ?? new Error("FCE_DEACTIVATE_FAILED", "Failed to deactivate prior plan."));
                }

                // Create new active plan
                var createResult = await _mediator.Send(new CreateAssignedPlanCommand(request.UserId, newPlanConfig.Id), cancellationToken);
                if (!createResult.IsSuccess)
                {
                    return Result.Rollback(createResult.Error ?? new Error("FCE_ASSIGN_FAILED", "Failed to assign new workout plan."));
                }

                // Append audit history log
                var historyResult = await _mediator.Send(new AppendPlanHistoryCommand(
                    request.UserId,
                    newPlanConfig.PlanId,
                    activePlan?.PlanId,
                    latestStats.Goal,
                    newStatus,
                    previousStatus,
                    metabolicResult.Bmr,
                    metabolicResult.Tdee,
                    metabolicResult.CalorieTarget,
                    createResult.Value,
                    request.TriggeredBy,
                    request.Reason ?? $"Weight update shifted status from {previousStatus} to {newStatus}"
                ), cancellationToken);

                if (!historyResult.IsSuccess)
                {
                    return Result.Rollback(historyResult.Error ?? new Error("FCE_HISTORY_FAILED", "Failed to record plan history audit."));
                }

                planReassigned = true;
            }

            // 6. Stage domain events in Transactional Outbox (persisted atomically with UoW commit)
            await _publishEndpoint.Publish(new FitnessMetricsRecalculatedEvent
            {
                UserId = request.UserId,
                Bmr = metabolicResult.Bmr,
                Tdee = metabolicResult.Tdee,
                CalorieTarget = metabolicResult.CalorieTarget,
                Status = newStatus.ToString(),
                PreviousStatus = previousStatus.ToString(),
                CalculatedAt = DateTime.UtcNow
            }, cancellationToken);

            if (planReassigned && !string.IsNullOrWhiteSpace(assignedPlanId))
            {
                await _publishEndpoint.Publish(new FitnessPlanAssignedEvent
                {
                    UserId = request.UserId,
                    PlanId = assignedPlanId,
                    PreviousPlanId = previousPlanId,
                    Reason = request.Reason ?? $"Weight update shifted status from {previousStatus} to {newStatus}",
                    AssignedAt = DateTime.UtcNow
                }, cancellationToken);

                await _publishEndpoint.Publish(new PlanAssignedEvent
                {
                    UserId = request.UserId,
                    PlanId = assignedPlanId,
                    AssignedAt = DateTime.UtcNow
                }, cancellationToken);
            }

            return Result.Success();
        }, cancellationToken);

        if (!transactionResult.IsSuccess)
        {
            return Result<RecalculateMetricsResponse>.Failure(transactionResult.Error);
        }

        await _cache.RemoveAsync($"metrics:user:{request.UserId}", cancellationToken);

        var response = new RecalculateMetricsResponse(
            request.UserId,
            previousStatus.ToString(),
            newStatus.ToString(),
            metabolicResult.Bmr,
            metabolicResult.Tdee,
            metabolicResult.CalorieTarget,
            planReassigned,
            assignedPlanId,
            assignedPlanName,
            DateTime.UtcNow
        );

        return Result<RecalculateMetricsResponse>.Success(response);
    }
}
