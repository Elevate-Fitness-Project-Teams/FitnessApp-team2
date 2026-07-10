using FitnessCalculationService.Common;
using MediatR;

namespace FitnessCalculationService.Features.Calculations.Queries.GetUserMetrics;

public record GetUserMetricsQuery(string UserId) : IRequest<Result<GetUserMetricsResponse>>;
