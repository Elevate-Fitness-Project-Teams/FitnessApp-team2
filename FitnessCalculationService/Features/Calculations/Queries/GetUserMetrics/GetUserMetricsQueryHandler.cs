using FitnessCalculationService.Common;
using FitnessCalculationService.Common.Caching;
using FitnessCalculationService.Common.Errors;
using FitnessCalculationService.Domain.Entities;
using FitnessCalculationService.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitnessCalculationService.Features.Calculations.Queries.GetUserMetrics;

public class GetUserMetricsQueryHandler : IRequestHandler<GetUserMetricsQuery, Result<GetUserMetricsResponse>>
{
    private readonly IGenericRepository<CalculatedMetrics> _repository;
    private readonly ICacheService _cache;

    public GetUserMetricsQueryHandler(
        IGenericRepository<CalculatedMetrics> repository,
        ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<Result<GetUserMetricsResponse>> Handle(GetUserMetricsQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.UserId, out var userGuid))
        {
            return Result<GetUserMetricsResponse>.Failure(new Error(ErrorCodes.FceMetricsNotCalculated, $"Invalid UserId: {request.UserId}"));
        }

        var cacheKey = $"metrics:user:{userGuid}";
        var cached = await _cache.GetAsync<GetUserMetricsResponse>(cacheKey, cancellationToken);
        if (cached != null)
        {
            return Result<GetUserMetricsResponse>.Success(cached);
        }

        var metrics = await _repository.GetQueryable()
            .AsNoTracking()
            .Where(m => m.UserId == userGuid)
            .Select(m => new GetUserMetricsResponse(
                m.UserId.ToString(),
                m.Bmr,
                m.Tdee,
                m.CalorieTarget,
                m.Status.ToString(),
                m.CalculatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (metrics == null)
        {
            return Result<GetUserMetricsResponse>.Failure(new Error(ErrorCodes.FceMetricsNotCalculated, $"Metrics not calculated yet for user {request.UserId}"));
        }

        await _cache.SetAsync(cacheKey, metrics, TimeSpan.FromMinutes(5), cancellationToken);

        return Result<GetUserMetricsResponse>.Success(metrics);
    }
}

