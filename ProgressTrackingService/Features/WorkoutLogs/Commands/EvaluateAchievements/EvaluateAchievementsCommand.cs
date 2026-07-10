/*
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Features.WorkoutLogs.Queries.GetAvailableAchievements;
using ProgressTrackingService.Features.WorkoutLogs.Queries.GetUserStatistic;
using ProgressTrackingService.MessageBroker.Events;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.WorkoutLogs.Commands.EvaluateAchievements;

public class EvaluateAchievementsCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
}

public class EvaluateAchievementsHandler : IRequestHandler<EvaluateAchievementsCommand, Result>
{
    private readonly IGeneralRepo<UserAchievement> _userAchievementRepo;
    private readonly IMediator _mediator;
    private readonly IPublishEndpoint _publishEndpoint;

    public EvaluateAchievementsHandler(
        IGeneralRepo<UserAchievement> userAchievementRepo,
        IMediator mediator,
        IPublishEndpoint publishEndpoint)
    {
        _userAchievementRepo = userAchievementRepo;
        _mediator = mediator;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result> Handle(EvaluateAchievementsCommand request, CancellationToken cancellationToken)
    {
        var statsResult = await _mediator.Send(new GetUserStatisticQuery { UserId = request.UserId }, cancellationToken);
        var stats = statsResult.IsSuccess ? statsResult.Value : null;
        var totalWorkouts = stats?.TotalWorkouts ?? 0;

        var userAchievements = await _userAchievementRepo.Find(ua =>ua ua.UserId == request.UserId)
            .Select(ua => ua.AchievementId)
            .ToListAsync(cancellationToken);

        var allAchievementsResult = await _mediator.Send(new GetAvailableAchievementsQuery { UserId = request.UserId }, cancellationToken);
        var allAchievements = allAchievementsResult.IsSuccess ? allAchievementsResult.Value : new List<GetAvailableAchievementResponse>();
        var availableAchievements = allAchievements.Where(a => !userAchievements.Contains(a.Id)).ToList();
        
        foreach (var achievement in availableAchievements)
        {
            bool earned = false;
            if (achievement.Name == "First Workout" && totalWorkouts >= 1) earned = true;
            if (achievement.Name == "10 Workouts" && totalWorkouts >= 10) earned = true;
            if (achievement.Name == "50 Workouts" && totalWorkouts >= 50) earned = true;
            if (achievement.Name == "100 Workouts" && totalWorkouts >= 100) earned = true;

            if (earned)
            {
                var newUa = new UserAchievement
                {
                    UserId = request.UserId,
                    AchievementId = achievement.Id,
                    EarnedAt = DateTime.UtcNow
                };
                await _userAchievementRepo.AddAsync(newUa, cancellationToken);

                await _publishEndpoint.Publish(new AchievementEarnedEvent
                {
                    UserId = request.UserId,
                    AchievementId = achievement.Id,
                    AchievementName = achievement.Name,
                    EarnedAt = newUa.EarnedAt
                }, cancellationToken);
            }
        }

        return Result.Success();
    }
}
*/
