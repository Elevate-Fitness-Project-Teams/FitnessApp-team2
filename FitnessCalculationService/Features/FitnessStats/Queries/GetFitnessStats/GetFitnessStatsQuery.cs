using FitnessCalculationService.Common;
using MediatR;

namespace FitnessCalculationService.Features.FitnessStats.Queries.GetFitnessStats;

public record GetFitnessStatsQuery(string UserId) : IRequest<Result<GetFitnessStatsResponse>>;
