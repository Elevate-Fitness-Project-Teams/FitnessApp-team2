using FitnessCalculationService.Common;
using FitnessCalculationService.Common.Exceptions;
using FitnessCalculationService.Domain.Entities;
using FitnessCalculationService.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitnessCalculationService.Features.FitnessStats.Queries.GetFitnessStats;

public class GetFitnessStatsQueryHandler : IRequestHandler<GetFitnessStatsQuery, Result<GetFitnessStatsResponse>>
{
    private readonly IGenericRepository<UserFitnessStats> _repository;

    public GetFitnessStatsQueryHandler(IGenericRepository<UserFitnessStats> repository)
    {
        _repository = repository;
    }

    public async Task<Result<GetFitnessStatsResponse>> Handle(GetFitnessStatsQuery request, CancellationToken cancellationToken)
    {
        var stats = await _repository.GetQueryable()
            .Where(s => s.UserId == request.UserId)
            .Select(s => new GetFitnessStatsResponse(
                s.UserId,
                s.Weight,
                s.Height,
                s.Age,
                s.Gender.ToString(),
                s.Goal.ToString(),
                s.ActivityLevel.ToString(),
                s.RecordedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (stats == null)
            throw new NotFoundException($"Fitness stats not found for user {request.UserId}", "FCE_STATS_NOT_FOUND");

        return Result<GetFitnessStatsResponse>.Success(stats);
    }
}
