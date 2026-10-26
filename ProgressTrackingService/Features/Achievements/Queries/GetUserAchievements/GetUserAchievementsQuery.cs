using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Caching;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Features.Achievements.DTOs;
using ProgressTrackingService.Features.Achievements.Queries.GetAchievementCatalog;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.Achievements.Queries.GetUserAchievements;

public record GetUserAchievementsQuery(Guid UserId, bool IncludeLocked = false) : IRequest<Result<List<UserAchievementDto>>>;

public class GetUserAchievementsHandler : IRequestHandler<GetUserAchievementsQuery, Result<List<UserAchievementDto>>>
{
    private readonly IGeneralRepo<UserAchievement> _userAchievementRepo;
    private readonly IMediator _mediator;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetUserAchievementsHandler> _logger;

    public GetUserAchievementsHandler(
        IGeneralRepo<UserAchievement> userAchievementRepo,
        IMediator mediator,
        ICacheService cacheService,
        ILogger<GetUserAchievementsHandler> logger)
    {
        _userAchievementRepo = userAchievementRepo;
        _mediator = mediator;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<List<UserAchievementDto>>> Handle(
        GetUserAchievementsQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = ProgressCacheKeys.UserAchievements(request.UserId);

        // Fetch user's earned achievements (cache-aside when not including locked)
        List<UserAchievementDto>? earned = null;

        if (!request.IncludeLocked)
        {
            try
            {
                earned = await _cacheService.GetAsync<List<UserAchievementDto>>(cacheKey, cancellationToken);
                if (earned != null)
                {
                    _logger.LogDebug("Cache hit for user achievements: {UserId}", request.UserId);
                    return Result.Success(earned);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read user achievements from cache for user {UserId}", request.UserId);
            }
        }

        earned = await _userAchievementRepo.Find(ua => ua.UserId == request.UserId)
            .Include(ua => ua.Achievement)
            .AsNoTracking()
            .OrderByDescending(ua => ua.EarnedAt)
            .Select(ua => new UserAchievementDto
            {
                Id = ua.AchievementId,
                Code = ua.Achievement.Code,
                Name = ua.Achievement.Name,
                Description = ua.Achievement.Description,
                IconUrl = ua.Achievement.IconUrl,
                CriteriaType = ua.Achievement.CriteriaType,
                Threshold = ua.Achievement.Threshold,
                Earned = true,
                EarnedAt = ua.EarnedAt
            })
            .ToListAsync(cancellationToken);

        // Populate cache for unlocked achievements
        try
        {
            await _cacheService.SetAsync(cacheKey, earned, ProgressCacheKeys.UserAchievementsTtl, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cache user achievements for user {UserId}", request.UserId);
        }

        if (!request.IncludeLocked)
        {
            return Result.Success(earned);
        }

        // When includeLocked is true, merge with complete catalog
        var catalogResult = await _mediator.Send(new GetAchievementCatalogQuery(), cancellationToken);
        var catalog = catalogResult.IsSuccess ? catalogResult.Value : new List<AchievementRuleDto>();

        var earnedDict = earned.ToDictionary(e => e.Id, e => e.EarnedAt);

        var merged = catalog.Select(rule => new UserAchievementDto
        {
            Id = rule.Id,
            Code = rule.Code,
            Name = rule.Name,
            Description = rule.Description,
            IconUrl = rule.IconUrl,
            CriteriaType = rule.CriteriaType,
            Threshold = rule.Threshold,
            Earned = earnedDict.ContainsKey(rule.Id),
            EarnedAt = earnedDict.TryGetValue(rule.Id, out var earnedAt) ? earnedAt : null
        })
        .OrderByDescending(a => a.Earned)
        .ThenByDescending(a => a.EarnedAt)
        .ToList();

        return Result.Success(merged);
    }
}
