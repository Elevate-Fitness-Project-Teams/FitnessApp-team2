using FitnessCalculationService.Common;
using FitnessCalculationService.Common.Exceptions;
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
        var metrics = await _repository.GetQueryable()
            .Where(m => m.UserId == request.UserId)
            .Select(m => new GetUserMetricsResponse(
                m.UserId,
                m.Bmr,
                m.Tdee,
                m.CalorieTarget,
                m.CalculatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (metrics == null)
            throw new NotFoundException($"Metrics not found for user {request.UserId}", "FCE_METRICS_NOT_FOUND");

        return Result<GetUserMetricsResponse>.Success(metrics);
    }
}
