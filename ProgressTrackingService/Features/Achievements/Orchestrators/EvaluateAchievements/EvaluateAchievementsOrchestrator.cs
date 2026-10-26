using MediatR;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Domain;
using ProgressTrackingService.Features.Achievements.Commands.AwardAchievement;
using ProgressTrackingService.Features.Achievements.DTOs;
using ProgressTrackingService.Features.Achievements.Queries.GetAchievementCatalog;
using ProgressTrackingService.Features.Achievements.Queries.GetAchievementProgressSnapshot;
using ProgressTrackingService.Features.Achievements.Queries.GetEarnedAchievementIds;

namespace ProgressTrackingService.Features.Achievements.Orchestrators.EvaluateAchievements;

public record EvaluateAchievementsOrchestrator(Guid UserId, DateTime Timestamp) : IRequest<Result<List<EarnedAchievementDto>>>;

public class EvaluateAchievementsHandler : IRequestHandler<EvaluateAchievementsOrchestrator, Result<List<EarnedAchievementDto>>>
{
    private readonly IMediator _mediator;
    private readonly ILogger<EvaluateAchievementsHandler> _logger;

    public EvaluateAchievementsHandler(IMediator mediator, ILogger<EvaluateAchievementsHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Result<List<EarnedAchievementDto>>> Handle(
        EvaluateAchievementsOrchestrator request,
        CancellationToken cancellationToken)
    {
        var catalogResult = await _mediator.Send(new GetAchievementCatalogQuery(), cancellationToken);
        if (!catalogResult.IsSuccess)
        {
            return Result.Failure<List<EarnedAchievementDto>>(catalogResult.Error);
        }

        var earnedIdsResult = await _mediator.Send(new GetEarnedAchievementIdsQuery(request.UserId), cancellationToken);
        if (!earnedIdsResult.IsSuccess)
        {
            return Result.Failure<List<EarnedAchievementDto>>(earnedIdsResult.Error);
        }

        var snapshotResult = await _mediator.Send(new GetAchievementProgressSnapshotQuery(request.UserId), cancellationToken);
        if (!snapshotResult.IsSuccess)
        {
            return Result.Failure<List<EarnedAchievementDto>>(snapshotResult.Error);
        }

        var qualifyingRules = AchievementRuleEvaluator.Evaluate(
            snapshotResult.Value,
            catalogResult.Value,
            earnedIdsResult.Value);

        var newlyEarned = new List<EarnedAchievementDto>();

        foreach (var rule in qualifyingRules)
        {
            var awardResult = await _mediator.Send(new AwardAchievementCommand(
                request.UserId,
                rule.Id,
                rule.Name,
                request.Timestamp), cancellationToken);

            if (awardResult.IsSuccess)
            {
                newlyEarned.Add(new EarnedAchievementDto(
                    rule.Id,
                    rule.Code,
                    rule.Name,
                    rule.Description,
                    rule.IconUrl,
                    request.Timestamp));
            }
            else
            {
                _logger.LogWarning("Failed to award achievement {RuleId} to user {UserId}: {Error}",
                    rule.Id, request.UserId, awardResult.Error.Description);
            }
        }

        return Result.Success(newlyEarned);
    }
}
