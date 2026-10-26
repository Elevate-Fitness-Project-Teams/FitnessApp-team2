using MassTransit;
using MediatR;
using MessageBroker.Events;
using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.Achievements.Commands.AwardAchievement;

public record AwardAchievementCommand(
    Guid UserId,
    Guid AchievementId,
    string AchievementName,
    DateTime EarnedAt) : IRequest<Result<Guid>>;

public class AwardAchievementHandler : IRequestHandler<AwardAchievementCommand, Result<Guid>>
{
    private readonly IGeneralRepo<UserAchievement> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<AwardAchievementHandler> _logger;

    public AwardAchievementHandler(
        IGeneralRepo<UserAchievement> repo,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint,
        ILogger<AwardAchievementHandler> logger)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        AwardAchievementCommand request,
        CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            // Idempotency check: verify if already awarded
            var exists = await _repo.Find(ua =>
                ua.UserId == request.UserId &&
                ua.AchievementId == request.AchievementId)
                .AnyAsync(cancellationToken);

            if (exists)
            {
                _logger.LogInformation(
                    "Achievement {AchievementId} already earned by user {UserId}. Skipping duplicate award.",
                    request.AchievementId, request.UserId);
                return Result.Success(request.AchievementId);
            }

            var record = new UserAchievement
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                AchievementId = request.AchievementId,
                EarnedAt = request.EarnedAt
            };

            try
            {
                await _repo.AddAsync(record, cancellationToken);

                // Stage event in MassTransit EF transactional outbox
                await _publishEndpoint.Publish(new AchievementEarnedEvent
                {
                    UserId = request.UserId,
                    AchievementId = request.AchievementId,
                    AchievementName = request.AchievementName,
                    EarnedAt = request.EarnedAt
                }, cancellationToken);

                _logger.LogInformation(
                    "Achievement {AchievementName} ({AchievementId}) awarded to user {UserId}.",
                    request.AchievementName, request.AchievementId, request.UserId);

                return Result.Success(record.Id);
            }
            catch (DbUpdateException ex)
            {
                // Concurrency defense: unique index on (UserId, AchievementId)
                _logger.LogWarning(ex,
                    "Unique constraint hit when awarding achievement {AchievementId} to user {UserId}. Treated as already awarded.",
                    request.AchievementId, request.UserId);
                return Result.Success(request.AchievementId);
            }
        }, cancellationToken);
    }
}
