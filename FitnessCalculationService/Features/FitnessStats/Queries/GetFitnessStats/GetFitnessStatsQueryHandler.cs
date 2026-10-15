using FitnessCalculationService.Common;
using FitnessCalculationService.Common.Errors;
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
        if (!Guid.TryParse(request.UserId, out var userGuid))
        {
            return Result<GetFitnessStatsResponse>.Failure(new Error(ErrorCodes.FceStatsNotFound, $"Invalid UserId: {request.UserId}"));
        }

        var stats = await _repository.GetQueryable()
            .AsNoTracking()
            .Where(s => s.UserId == userGuid)
            .OrderByDescending(s => s.RecordedAt)
            .Select(s => new GetFitnessStatsResponse(
                s.UserId.ToString(),
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
        {
            return Result<GetFitnessStatsResponse>.Failure(new Error(ErrorCodes.FceStatsNotFound, $"Fitness stats not found for user {request.UserId}"));
        }

        return Result<GetFitnessStatsResponse>.Success(stats);
    }
}
