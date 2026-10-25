using MediatR;
using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Caching;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Features.Achievements.DTOs;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.Achievements.Queries.GetAchievementCatalog;

public record GetAchievementCatalogQuery : IRequest<Result<List<AchievementRuleDto>>>;

public class GetAchievementCatalogHandler : IRequestHandler<GetAchievementCatalogQuery, Result<List<AchievementRuleDto>>>
{
    private readonly IGeneralRepo<Achievement> _repo;
    private readonly ICacheService _cacheService;

    public GetAchievementCatalogHandler(IGeneralRepo<Achievement> repo, ICacheService cacheService)
    {
        _repo = repo;
        _cacheService = cacheService;
    }

    public async Task<Result<List<AchievementRuleDto>>> Handle(
        GetAchievementCatalogQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = ProgressCacheKeys.AchievementsCatalog();
        var cached = await _cacheService.GetAsync<List<AchievementRuleDto>>(cacheKey, cancellationToken);
        if (cached != null && cached.Count > 0)
        {
            return Result.Success(cached);
        }

        var catalog = await _repo.GetAll()
            .Select(a => new AchievementRuleDto(
                a.Id,
                a.Code,
                a.Name,
                a.Description,
                a.IconUrl,
                a.CriteriaType,
                a.Threshold))
            .ToListAsync(cancellationToken);

        if (catalog.Count > 0)
        {
            await _cacheService.SetAsync(cacheKey, catalog, ProgressCacheKeys.AchievementsCatalogTtl, cancellationToken);
        }

        return Result.Success(catalog);
    }
}
