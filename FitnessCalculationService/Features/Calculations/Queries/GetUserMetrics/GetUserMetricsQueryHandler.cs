using FitnessCalculationService.Common;
using FitnessCalculationService.Common.Errors;
using FitnessCalculationService.Domain.Entities;
using FitnessCalculationService.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitnessCalculationService.Features.Calculations.Queries.GetUserMetrics;

public class GetUserMetricsQueryHandler : IRequestHandler<GetUserMetricsQuery, Result<GetUserMetricsResponse>>
{
    private readonly IGenericRepository<CalculatedMetrics> _repository;

    public GetUserMetricsQueryHandler(IGenericRepository<CalculatedMetrics> repository)
    {
        _repository = repository;
    }

    public async Task<Result<GetUserMetricsResponse>> Handle(GetUserMetricsQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.UserId, out var userGuid))
        {
            return Result<GetUserMetricsResponse>.Failure(new Error(ErrorCodes.FceMetricsNotCalculated, $"Invalid UserId: {request.UserId}"));
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

        return Result<GetUserMetricsResponse>.Success(metrics);
    }
}
